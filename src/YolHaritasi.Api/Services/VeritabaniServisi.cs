using System.Data.Common;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Dtos;

namespace YolHaritasi.Api.Services;

/// <summary>
/// Veritabanı ekranının arkasındaki servis.
///
/// Burada EF Core'un LINQ'i yerine doğrudan SQL yazıyoruz. Buna ADO.NET denir:
/// .NET'in veritabanıyla konuşmasının en alt seviyesi. EF Core da kendi içinde ADO.NET kullanır.
///   DbConnection → bağlantı      DbCommand → çalıştırılacak SQL      DbDataReader → satır satır sonuç
/// </summary>
public partial class VeritabaniServisi(UygulamaDbContext db, ILogger<VeritabaniServisi> logger)
{
    private const int MaksimumSatir = 200;
    private const int ZamanAsimiSaniye = 5;

    public async Task<VeritabaniDurumDto> DurumAsync()
    {
        // Bağlantı nesnesini EF Core'dan alıyoruz (aynı connection string).
        var baglanti = db.Database.GetDbConnection();

        // Dikkat: Connection string'in TAMAMINI asla dışarı göstermeyiz; içinde şifre olabilir.
        // Sadece sunucu ve veritabanı adı.
        var sunucu = baglanti.DataSource;
        var veritabani = baglanti.Database;

        try
        {
            if (!await db.Database.CanConnectAsync())
                return new VeritabaniDurumDto(false, "SQL Server", sunucu, veritabani, null, [], [], "Veritabanına bağlanılamadı.");

            await baglanti.OpenAsync();
            try
            {
                var surum = (await TekDegerAsync(baglanti, "SELECT @@VERSION"))?.Split('\n')[0].Trim();
                var migrationlar = (await db.Database.GetAppliedMigrationsAsync()).ToList();
                var tablolar = await TablolariOkuAsync(baglanti);
                return new VeritabaniDurumDto(true, "SQL Server", sunucu, veritabani, surum, migrationlar, tablolar, null);
            }
            finally
            {
                await baglanti.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Veritabanı durumu okunamadı");
            return new VeritabaniDurumDto(false, "SQL Server", sunucu, veritabani, null, [], [], ex.Message);
        }
    }

    /// <summary>
    /// SQL Server'ın SİSTEM KATALOĞUNDAN (sys.tables, sys.columns...) tablo ve sütun bilgilerini okur.
    /// Bu tablolar SQL Server'ın kendi hakkında tuttuğu bilgilerdir (metadata).
    /// </summary>
    private static async Task<List<TabloDto>> TablolariOkuAsync(DbConnection baglanti)
    {
        const string sutunSql = """
            SELECT t.name AS Tablo, c.name AS Sutun, ty.name AS Tip, c.max_length, c.is_nullable,
                   CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END AS BirincilAnahtar,
                   rt.name AS BagliTablo
            FROM sys.tables t
            JOIN sys.columns c ON c.object_id = t.object_id
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            LEFT JOIN (SELECT ic.object_id, ic.column_id
                       FROM sys.indexes i
                       JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                       WHERE i.is_primary_key = 1) pk
                   ON pk.object_id = c.object_id AND pk.column_id = c.column_id
            LEFT JOIN sys.foreign_key_columns fkc ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
            LEFT JOIN sys.tables rt ON rt.object_id = fkc.referenced_object_id
            ORDER BY t.name, c.column_id
            """;

        const string sayiSql = """
            SELECT t.name, SUM(p.rows)
            FROM sys.tables t
            JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
            GROUP BY t.name
            """;

        var sutunlar = new Dictionary<string, List<SutunDto>>();
        await using (var komut = baglanti.CreateCommand())
        {
            komut.CommandText = sutunSql;
            await using var okuyucu = await komut.ExecuteReaderAsync();
            while (await okuyucu.ReadAsync())   // her ReadAsync bir sonraki satıra geçer
            {
                var tablo = okuyucu.GetString(0);
                var tip = TipAdi(okuyucu.GetString(2), okuyucu.GetInt16(3));
                var sutun = new SutunDto(
                    okuyucu.GetString(1), tip, okuyucu.GetBoolean(4),
                    okuyucu.GetInt32(5) == 1, okuyucu.IsDBNull(6) ? null : okuyucu.GetString(6));

                if (!sutunlar.TryGetValue(tablo, out var liste))
                    sutunlar[tablo] = liste = [];
                liste.Add(sutun);
            }
        }

        var sayilar = new Dictionary<string, long>();
        await using (var komut = baglanti.CreateCommand())
        {
            komut.CommandText = sayiSql;
            await using var okuyucu = await komut.ExecuteReaderAsync();
            while (await okuyucu.ReadAsync())
                sayilar[okuyucu.GetString(0)] = Convert.ToInt64(okuyucu.GetValue(1));
        }

        return sutunlar
            .Select(s => new TabloDto(s.Key, sayilar.GetValueOrDefault(s.Key), s.Value))
            .ToList();
    }

    /// <summary>nvarchar + 400 bayt → "nvarchar(200)" gibi okunabilir tip adı.</summary>
    private static string TipAdi(string tip, short maxUzunluk) => tip switch
    {
        "nvarchar" or "nchar" => maxUzunluk == -1 ? $"{tip}(max)" : $"{tip}({maxUzunluk / 2})",   // n'li tipler karakter başına 2 bayt
        "varchar" or "char" or "varbinary" => maxUzunluk == -1 ? $"{tip}(max)" : $"{tip}({maxUzunluk})",
        _ => tip,
    };

    private static async Task<string?> TekDegerAsync(DbConnection baglanti, string sql)
    {
        await using var komut = baglanti.CreateCommand();
        komut.CommandText = sql;
        return (await komut.ExecuteScalarAsync())?.ToString();   // ExecuteScalar: ilk satırın ilk sütunu
    }

    // =====================================================================
    //  SQL PRATİK ALANI
    // =====================================================================

    // Yasaklı kelimeler: Veriyi veya yapıyı değiştirebilecek, sunucuda komut çalıştırabilecek her şey.
    // [GeneratedRegex]: Regex'i derleme zamanında üretir (daha hızlı). \b = kelime sınırı.
    [GeneratedRegex(@"\b(INSERT|UPDATE|DELETE|MERGE|DROP|ALTER|CREATE|TRUNCATE|EXEC|EXECUTE|GRANT|REVOKE|DENY|BACKUP|RESTORE|SHUTDOWN|INTO|OPENROWSET|OPENQUERY|OPENDATASOURCE|WAITFOR|DBCC|KILL|USE|xp_\w+|sp_\w+)\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex YasakliKelime();

    /// <summary>
    /// Sadece okuma (SELECT) sorgusu çalıştırır. Güvenlik için ÜÇ katman var:
    ///   1) Sorgu SELECT veya WITH ile başlamalı, yasaklı kelime ve ';' içermemeli
    ///   2) Sorgu bir TRANSACTION içinde çalışır ve sonunda her durumda ROLLBACK edilir
    ///      → bir şekilde değişiklik yapsa bile geri alınır
    ///   3) Zaman aşımı (5 sn) ve satır sınırı (200): Sunucuyu yoracak sorgular durdurulur
    /// Bu "derinlemesine savunma" (defense in depth) yaklaşımıdır: tek bir kontrole güvenilmez.
    /// Yine de bu özellik SADECE geliştirme ortamında açıktır (controller'a bak).
    /// </summary>
    public async Task<SorguSonucuDto> SorguCalistirAsync(string sql)
    {
        sql = sql.Trim().TrimEnd(';').Trim();

        if (sql.Length == 0)
            throw new SorguHatasi("Sorgu boş.");
        if (!(sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) || sql.StartsWith("WITH", StringComparison.OrdinalIgnoreCase)))
            throw new SorguHatasi("Sadece SELECT (veya WITH ile başlayan) okuma sorgularına izin veriliyor.");
        if (sql.Contains(';'))
            throw new SorguHatasi("Tek seferde sadece bir sorgu çalıştırılabilir (';' kullanma).");
        if (YasakliKelime().Match(sql) is { Success: true } eslesme)
            throw new SorguHatasi($"'{eslesme.Value}' ifadesine izin verilmiyor. Bu alan sadece veri okumak içindir.");

        var baglanti = db.Database.GetDbConnection();
        await baglanti.OpenAsync();
        await using var islem = await baglanti.BeginTransactionAsync();
        try
        {
            await using var komut = baglanti.CreateCommand();
            komut.Transaction = islem;
            komut.CommandText = sql;
            komut.CommandTimeout = ZamanAsimiSaniye;

            var sayac = Stopwatch.StartNew();
            await using var okuyucu = await komut.ExecuteReaderAsync();

            var sutunlar = Enumerable.Range(0, okuyucu.FieldCount).Select(okuyucu.GetName).ToList();
            var satirlar = new List<List<string?>>();
            var kesildi = false;

            while (await okuyucu.ReadAsync())
            {
                if (satirlar.Count == MaksimumSatir) { kesildi = true; break; }
                satirlar.Add(Enumerable.Range(0, okuyucu.FieldCount)
                    .Select(i => okuyucu.IsDBNull(i) ? null : Convert.ToString(okuyucu.GetValue(i)))
                    .ToList());
            }

            return new SorguSonucuDto(sutunlar, satirlar, satirlar.Count, kesildi, sayac.ElapsedMilliseconds);
        }
        catch (SqlException ex)
        {
            // SQL Server'ın kendi hata mesajı (ör. "Invalid column name 'Adi'") kullanıcıya gösterilir.
            throw new SorguHatasi($"SQL Server hatası: {ex.Message}");
        }
        finally
        {
            await islem.RollbackAsync();   // her durumda geri al
            await baglanti.CloseAsync();
        }
    }
}

/// <summary>Kullanıcının sorgusundaki hatalar için özel exception tipi (→ 400 Bad Request).</summary>
public class SorguHatasi(string mesaj) : Exception(mesaj);
