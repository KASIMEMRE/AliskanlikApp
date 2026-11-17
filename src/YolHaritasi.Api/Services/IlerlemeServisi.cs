using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Dtos;

namespace YolHaritasi.Api.Services;

/// <summary>
/// INTERFACE (arayüz): Servisin "ne yaptığını" tanımlar, "nasıl yaptığını" değil.
/// Controller bu interface'i bilir; arkadaki gerçek sınıfı bilmez.
/// Bu sayede ileride sınıfı değiştirmek veya test için sahte (mock) bir sürüm vermek kolaylaşır.
/// </summary>
public interface IIlerlemeServisi
{
    Task<BugunDto> BugunAsync();
    Task<KonuDetayDto?> KonuDetayAsync(int konuId);
    Task<bool> KonuyuTamamlaAsync(int konuId);
    Task<bool> GorevDurumuDegistirAsync(int gorevId, bool tamamlandi);
}

/// <summary>
/// SERVICE LAYER (servis katmanı): İŞ KURALLARI burada yaşar.
/// Örnek iş kuralı: "Bugünün konusu = sıradaki ilk tamamlanmamış konudur."
///
/// Constructor'daki "UygulamaDbContext db" parametresini biz vermiyoruz:
/// ASP.NET Core'un DEPENDENCY INJECTION sistemi her istek için bir DbContext oluşturup buraya veriyor.
/// (Program.cs'teki AddDbContext ve AddScoped satırlarına bak.)
/// </summary>
public class IlerlemeServisi(UygulamaDbContext db, ILogger<IlerlemeServisi> logger) : IIlerlemeServisi
{
    public async Task<BugunDto> BugunAsync()
    {
        var toplam = await db.Konular.CountAsync();
        var tamamlanan = await db.Konular.CountAsync(k => k.TamamlanmaTarihi != null);
        var yuzde = toplam == 0 ? 0 : tamamlanan * 100 / toplam;

        // Sıradaki konu: Önce alan sırasına, sonra konu sırasına göre ilk tamamlanmamış konu.
        // Bu LINQ sorgusu SQL Server'a aşağı yukarı şöyle gider:
        //   SELECT TOP(1) k.Id FROM Konular k
        //   INNER JOIN Alanlar a ON k.AlanId = a.Id
        //   WHERE k.TamamlanmaTarihi IS NULL
        //   ORDER BY a.Sira, k.Sira
        var siradakiId = await db.Konular
            .Where(k => k.TamamlanmaTarihi == null)
            .OrderBy(k => k.Alan.Sira)
            .ThenBy(k => k.Sira)
            .Select(k => (int?)k.Id)
            .FirstOrDefaultAsync();

        if (siradakiId is null)
            return new BugunDto("Tebrikler! Yol haritasındaki tüm konuları tamamladın. 🎉", yuzde, null);

        var konu = await KonuDetayAsync(siradakiId.Value);
        return new BugunDto($"Bugünün konusu: {konu!.Baslik}", yuzde, konu);
    }

    public async Task<KonuDetayDto?> KonuDetayAsync(int konuId)
    {
        // PROJECTION (Select ile DTO'ya çevirme): Sadece ihtiyaç duyulan sütunlar SQL'den çekilir.
        // Terimler ve görevler de aynı sorguda gelir; her biri için ayrı sorgu atılmaz (N+1 problemi yok).
        // AsNoTracking: Sadece okuma yapacağız, EF Core'un değişiklik takibine gerek yok → daha hızlı.
        return await db.Konular
            .AsNoTracking()
            .Where(k => k.Id == konuId)
            .Select(k => new KonuDetayDto(
                k.Id,
                k.Alan.Ad,
                k.Sira,
                k.Baslik,
                k.Ozet,
                k.TahminiDakika,
                k.TamamlanmaTarihi,
                k.Terimler.OrderBy(t => t.Id).Select(t => new TerimDto(t.Id, t.Ad, t.Tanim, t.Ornek)).ToList(),
                k.Gorevler.OrderBy(g => g.Sira).Select(g => new GorevDto(g.Id, g.Sira, g.Aciklama, g.Tamamlandi)).ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> KonuyuTamamlaAsync(int konuId)
    {
        // Burada AsNoTracking YOK: EF Core bu nesneyi takip etsin ki değişikliği fark edip UPDATE üretsin.
        var konu = await db.Konular.FindAsync(konuId);
        if (konu is null)
            return false;

        if (konu.TamamlanmaTarihi is null)
        {
            konu.TamamlanmaTarihi = DateTime.UtcNow;   // UTC: sunucunun saat diliminden bağımsız zaman
            await db.SaveChangesAsync();               // → UPDATE Konular SET TamamlanmaTarihi = ... WHERE Id = ...

            // STRUCTURED LOGGING: {KonuId} bir yer tutucudur; log sistemleri bunu ayrı bir alan olarak saklar
            // ve sonra "KonuId = 5 olan tüm loglar" diye arama yapabilirsin.
            logger.LogInformation("Konu tamamlandı. KonuId={KonuId}, Baslik={Baslik}", konu.Id, konu.Baslik);
        }

        return true;
    }

    public async Task<bool> GorevDurumuDegistirAsync(int gorevId, bool tamamlandi)
    {
        var gorev = await db.Gorevler.FindAsync(gorevId);
        if (gorev is null)
            return false;

        gorev.Tamamlandi = tamamlandi;
        await db.SaveChangesAsync();
        return true;
    }
}
