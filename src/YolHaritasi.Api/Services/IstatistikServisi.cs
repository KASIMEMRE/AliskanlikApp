using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Dtos;

namespace YolHaritasi.Api.Services;

/// <summary>
/// İstatistik hesapları. İki yaklaşımı yan yana görebilirsin:
///  1) Hesabı SQL Server'a yaptırmak (GROUP BY, SUM, COUNT): Büyük veride hızlıdır, sadece sonuç gelir.
///  2) Veriyi çekip C#'ta hesaplamak: SQL'de zor ifade edilen mantık için (ör. arka arkaya gün serisi).
/// Kural: Toplama/gruplama işini mümkün olduğunca veritabanına ver; satır satır mantığı C#'ta yap.
/// </summary>
public class IstatistikServisi(UygulamaDbContext db)
{
    // Kullanıcı Türkiye'de; "bugün" ve "gün" kavramları Türkiye saatine göre hesaplanmalı.
    // Veritabanında saatler UTC tutuluyor (sunucunun saat diliminden bağımsız olsun diye).
    private static readonly TimeZoneInfo Turkiye = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    private static DateOnly YerelGun(DateTime utc) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Turkiye));

    public async Task<IstatistikDto> GetirAsync(int kullaniciId)
    {
        var bugun = YerelGun(DateTime.UtcNow);

        // ---- 1) SQL'de toplama: tek değerler ----
        // SumAsync → SELECT COALESCE(SUM(Dakika), 0) FROM CalismaOturumlari WHERE KullaniciId = @k
        var oturumlar = db.CalismaOturumlari.Where(c => c.KullaniciId == kullaniciId);
        var toplamDakika = await oturumlar.SumAsync(c => c.Dakika);
        var haftaSiniri = DateTime.UtcNow.AddDays(-7);
        var buHafta = await oturumlar.Where(c => c.Baslangic >= haftaSiniri).SumAsync(c => c.Dakika);
        var tamamlanan = await db.KonuIlerlemeleri.CountAsync(i => i.KullaniciId == kullaniciId);
        var toplamKonu = await db.Konular.CountAsync();
        var notSayisi = await db.Notlar.CountAsync(n => n.KullaniciId == kullaniciId);

        // ---- 2) SQL'de GROUP BY: alan bazında dağılım ----
        // SELECT a.Ad, SUM(c.Dakika) FROM CalismaOturumlari c JOIN Konular k ... JOIN Alanlar a ...
        // WHERE c.KullaniciId = @k GROUP BY a.Ad
        var alanDakika = await oturumlar
            .GroupBy(c => c.Konu.Alan.Ad)
            .Select(g => new { Alan = g.Key, Dakika = g.Sum(c => c.Dakika) })
            .ToDictionaryAsync(x => x.Alan, x => x.Dakika);

        var alanlar = await db.Alanlar
            .OrderBy(a => a.Sira)
            .Select(a => new
            {
                a.Ad,
                Toplam = a.Konular.Count,
                Tamamlanan = a.Konular.Count(k => k.Ilerlemeler.Any(i => i.KullaniciId == kullaniciId)),
            })
            .ToListAsync();

        // ---- 3) C#'ta hesap: son 14 gün ve seriler ----
        // Önce SQL'den sadece gereken sütunları çekiyoruz (tarih + dakika), sonra Türkiye saatine göre gruplayıp
        // seriyi hesaplıyoruz. "Arka arkaya kaç gün" mantığını SQL'de yazmak mümkün ama okunması çok zor.
        var oturumGunleri = await oturumlar.Select(c => new { c.Baslangic, c.Dakika }).ToListAsync();
        var tamamlamaGunleri = await db.KonuIlerlemeleri
            .Where(i => i.KullaniciId == kullaniciId)
            .Select(i => i.TamamlanmaTarihi)
            .ToListAsync();

        var gunlukDakika = oturumGunleri
            .GroupBy(o => YerelGun(o.Baslangic))
            .ToDictionary(g => g.Key, g => g.Sum(o => o.Dakika));

        var son14 = Enumerable.Range(0, 14)
            .Select(i => bugun.AddDays(i - 13))
            .Select(g => new GunlukDakikaDto(g, gunlukDakika.GetValueOrDefault(g)))
            .ToList();

        // "Çalışılan gün" = o gün çalışma kaydı VEYA konu tamamlama var
        var aktifGunler = gunlukDakika.Keys
            .Concat(tamamlamaGunleri.Select(YerelGun))
            .ToHashSet();

        return new IstatistikDto(
            toplamDakika,
            buHafta,
            tamamlanan,
            toplamKonu,
            notSayisi,
            GuncelSeri(aktifGunler, bugun),
            EnUzunSeri(aktifGunler),
            son14,
            alanlar.Select(a => new AlanIstatistikDto(a.Ad, alanDakika.GetValueOrDefault(a.Ad), a.Tamamlanan, a.Toplam)).ToList());
    }

    /// <summary>Bugünden (bugün henüz çalışılmadıysa dünden) geriye doğru arka arkaya aktif gün sayısı.</summary>
    private static int GuncelSeri(HashSet<DateOnly> gunler, DateOnly bugun)
    {
        var gun = gunler.Contains(bugun) ? bugun : bugun.AddDays(-1);
        var seri = 0;
        while (gunler.Contains(gun))
        {
            seri++;
            gun = gun.AddDays(-1);
        }
        return seri;
    }

    /// <summary>Tüm zamanların en uzun serisi.</summary>
    private static int EnUzunSeri(HashSet<DateOnly> gunler)
    {
        var enUzun = 0;
        foreach (var gun in gunler)
        {
            if (gunler.Contains(gun.AddDays(-1))) continue;   // serinin başı değil
            var uzunluk = 1;
            while (gunler.Contains(gun.AddDays(uzunluk))) uzunluk++;
            enUzun = Math.Max(enUzun, uzunluk);
        }
        return enUzun;
    }
}
