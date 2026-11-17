using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Dtos;
using YolHaritasi.Api.Entities;

namespace YolHaritasi.Api.Services;

/// <summary>
/// INTERFACE (arayüz): Servisin "ne yaptığını" tanımlar, "nasıl yaptığını" değil.
/// Controller bu interface'i bilir; arkadaki gerçek sınıfı bilmez.
/// Bu sayede ileride sınıfı değiştirmek veya test için sahte (mock) bir sürüm vermek kolaylaşır.
///
/// Adım 4: Her metot artık "kullaniciId" alıyor; çünkü ilerleme kişiye özel.
/// </summary>
public interface IIlerlemeServisi
{
    Task<BugunDto> BugunAsync(int kullaniciId);
    Task<KonuDetayDto?> KonuDetayAsync(int kullaniciId, int konuId);
    Task<bool> KonuyuTamamlaAsync(int kullaniciId, int konuId);
    Task<bool> GorevDurumuDegistirAsync(int kullaniciId, int gorevId, bool tamamlandi);
}

/// <summary>
/// SERVICE LAYER (servis katmanı): İŞ KURALLARI burada yaşar.
/// Örnek iş kuralı: "Bugünün konusu = KULLANICININ henüz tamamlamadığı ilk konudur."
///
/// Constructor'daki "UygulamaDbContext db" parametresini biz vermiyoruz:
/// ASP.NET Core'un DEPENDENCY INJECTION sistemi her istek için bir DbContext oluşturup buraya veriyor.
/// (Program.cs'teki AddDbContext ve AddScoped satırlarına bak.)
/// </summary>
public class IlerlemeServisi(UygulamaDbContext db, ILogger<IlerlemeServisi> logger) : IIlerlemeServisi
{
    public async Task<BugunDto> BugunAsync(int kullaniciId)
    {
        var toplam = await db.Konular.CountAsync();
        var tamamlanan = await db.KonuIlerlemeleri.CountAsync(i => i.KullaniciId == kullaniciId);
        var yuzde = toplam == 0 ? 0 : tamamlanan * 100 / toplam;

        // Sıradaki konu: Bu kullanıcının İLERLEME KAYDI OLMAYAN ilk konu (alan sırası, sonra konu sırası).
        // SQL karşılığı (NOT EXISTS alt sorgusu):
        //   SELECT TOP(1) k.Id FROM Konular k
        //   INNER JOIN Alanlar a ON k.AlanId = a.Id
        //   WHERE NOT EXISTS (SELECT 1 FROM KonuIlerlemeleri i WHERE i.KonuId = k.Id AND i.KullaniciId = @kullaniciId)
        //   ORDER BY a.Sira, k.Sira
        var siradakiId = await db.Konular
            .Where(k => !k.Ilerlemeler.Any(i => i.KullaniciId == kullaniciId))
            .OrderBy(k => k.Alan.Sira)
            .ThenBy(k => k.Sira)
            .Select(k => (int?)k.Id)
            .FirstOrDefaultAsync();

        if (siradakiId is null)
            return new BugunDto("Tebrikler! Yol haritasındaki tüm konuları tamamladın. 🎉", yuzde, null);

        var konu = await KonuDetayAsync(kullaniciId, siradakiId.Value);
        return new BugunDto($"Bugünün konusu: {konu!.Baslik}", yuzde, konu);
    }

    public async Task<KonuDetayDto?> KonuDetayAsync(int kullaniciId, int konuId)
    {
        // PROJECTION (Select ile DTO'ya çevirme): Sadece ihtiyaç duyulan sütunlar SQL'den çekilir.
        // Terimler, görevler, kaynaklar ve kullanıcının ilerlemesi aynı sorguda gelir (N+1 problemi yok).
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
                // Bu kullanıcının tamamlama tarihi (yoksa null)
                k.Ilerlemeler.Where(i => i.KullaniciId == kullaniciId).Select(i => (DateTime?)i.TamamlanmaTarihi).FirstOrDefault(),
                k.DersNotu,
                k.Terimler.OrderBy(t => t.Id).Select(t => new TerimDto(t.Id, t.Ad, t.Tanim, t.Ornek, t.Kullanim, t.KodOrnegi, t.KodDili)).ToList(),
                k.Gorevler.OrderBy(g => g.Sira).Select(g => new GorevDto(
                    g.Id, g.Sira, g.Aciklama,
                    g.Ilerlemeler.Any(i => i.KullaniciId == kullaniciId))).ToList(),
                k.Kaynaklar.OrderBy(x => x.Sira).Select(x => new KaynakDto(x.Baslik, x.Url, x.Site)).ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> KonuyuTamamlaAsync(int kullaniciId, int konuId)
    {
        if (!await db.Konular.AnyAsync(k => k.Id == konuId))
            return false;

        // Zaten tamamlanmışsa tekrar ekleme (composite key zaten engellerdi ama hata yerine sessizce geç)
        var zatenVar = await db.KonuIlerlemeleri.AnyAsync(i => i.KullaniciId == kullaniciId && i.KonuId == konuId);
        if (!zatenVar)
        {
            db.KonuIlerlemeleri.Add(new KonuIlerleme { KullaniciId = kullaniciId, KonuId = konuId });
            await db.SaveChangesAsync();   // → INSERT INTO KonuIlerlemeleri ...

            // STRUCTURED LOGGING: {KonuId} bir yer tutucudur; log sistemleri bunu ayrı bir alan olarak saklar.
            logger.LogInformation("Konu tamamlandı. KullaniciId={KullaniciId}, KonuId={KonuId}", kullaniciId, konuId);
        }

        return true;
    }

    public async Task<bool> GorevDurumuDegistirAsync(int kullaniciId, int gorevId, bool tamamlandi)
    {
        if (!await db.Gorevler.AnyAsync(g => g.Id == gorevId))
            return false;

        if (tamamlandi)
        {
            if (!await db.GorevIlerlemeleri.AnyAsync(i => i.KullaniciId == kullaniciId && i.GorevId == gorevId))
            {
                db.GorevIlerlemeleri.Add(new GorevIlerleme { KullaniciId = kullaniciId, GorevId = gorevId });
                await db.SaveChangesAsync();
            }
        }
        else
        {
            // ExecuteDeleteAsync: Önce okumadan doğrudan DELETE gönderir.
            //   DELETE FROM GorevIlerlemeleri WHERE KullaniciId = @k AND GorevId = @g
            await db.GorevIlerlemeleri
                .Where(i => i.KullaniciId == kullaniciId && i.GorevId == gorevId)
                .ExecuteDeleteAsync();
        }

        return true;
    }
}
