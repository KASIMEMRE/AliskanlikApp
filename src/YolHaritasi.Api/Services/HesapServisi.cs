using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Auth;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Dtos;
using YolHaritasi.Api.Entities;
using YolHaritasi.Api.Hatalar;

namespace YolHaritasi.Api.Services;

/// <summary>
/// Kayıt ve giriş işlemleri.
/// Şifre hash'leme için ASP.NET Core Identity'nin PasswordHasher sınıfını kullanıyoruz:
/// PBKDF2 algoritması + rastgele SALT + 100.000 tekrar. Kendi hash algoritmanı ASLA yazma.
/// </summary>
public class HesapServisi(
    UygulamaDbContext db,
    TokenServisi tokenServisi,
    ILogger<HesapServisi> logger)
{
    private readonly PasswordHasher<Kullanici> _hasher = new();

    public async Task<OturumDto> KayitAsync(KayitIstegi istek)
    {
        var email = istek.Email.Trim().ToLowerInvariant();

        if (await db.Kullanicilar.AnyAsync(k => k.Email == email))
            throw new HesapHatasi("Bu e-posta adresiyle zaten bir hesap var.", StatusCodes.Status409Conflict);

        // İlk kayıt olan kişi Admin olur (uygulamanın sahibi). Sonrakiler normal kullanıcı.
        var ilkKullaniciMi = !await db.Kullanicilar.AnyAsync();

        var kullanici = new Kullanici
        {
            Ad = istek.Ad.Trim(),
            Email = email,
            Rol = ilkKullaniciMi ? Roller.Admin : Roller.Kullanici,
        };
        kullanici.SifreHash = _hasher.HashPassword(kullanici, istek.Sifre);

        db.Kullanicilar.Add(kullanici);
        await db.SaveChangesAsync();   // → INSERT; kullanici.Id artık dolu

        if (ilkKullaniciMi)
            await EskiIlerlemeyiTasiAsync(kullanici.Id);

        logger.LogInformation("Yeni kullanıcı kaydı. KullaniciId={KullaniciId}, Rol={Rol}", kullanici.Id, kullanici.Rol);
        return OturumOlustur(kullanici);
    }

    public async Task<OturumDto> GirisAsync(GirisIstegi istek)
    {
        var email = istek.Email.Trim().ToLowerInvariant();
        var kullanici = await db.Kullanicilar.SingleOrDefaultAsync(k => k.Email == email);

        // GÜVENLİK: "E-posta yok" ve "şifre yanlış" için AYNI mesajı veriyoruz.
        // Farklı mesaj verirsek saldırgan hangi e-postaların kayıtlı olduğunu öğrenebilir (user enumeration).
        if (kullanici is null ||
            _hasher.VerifyHashedPassword(kullanici, kullanici.SifreHash, istek.Sifre) == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Başarısız giriş denemesi. Email={Email}", email);
            throw new HesapHatasi("E-posta veya şifre hatalı.", StatusCodes.Status401Unauthorized);
        }

        return OturumOlustur(kullanici);
    }

    public async Task<KullaniciDto?> GetirAsync(int kullaniciId) =>
        await db.Kullanicilar
            .Where(k => k.Id == kullaniciId)
            .Select(k => new KullaniciDto(k.Id, k.Ad, k.Email, k.Rol))
            .SingleOrDefaultAsync();

    private OturumDto OturumOlustur(Kullanici k)
    {
        var (token, bitis) = tokenServisi.Uret(k);
        return new OturumDto(token, bitis, new KullaniciDto(k.Id, k.Ad, k.Email, k.Rol));
    }

    /// <summary>
    /// VERİ TAŞIMA (data migration): Adım 4'ten önce ilerleme tek kişilikti ve doğrudan
    /// Konular/Gorevler/ProjeAdimlari tablolarındaydı. Migration bu eski ilerlemeyi geçici
    /// "EskiIlerleme" tablosuna kopyaladı. İlk kayıt olan kullanıcı uygulamanın sahibi olduğu için
    /// o ilerlemeyi ona aktarıyor ve geçici tabloyu siliyoruz.
    ///
    /// Burada EF Core'un LINQ'i yerine HAM SQL kullanıyoruz, çünkü EskiIlerleme tablosu
    /// EF Core modelinde (DbContext'te) yok; sadece bu geçiş için var.
    /// {kullaniciId} → EF Core bunu otomatik PARAMETREYE (@p0) çevirir; SQL injection'a karşı güvenli.
    /// </summary>
    private async Task EskiIlerlemeyiTasiAsync(int kullaniciId)
    {
        var tasinan = await db.Database.ExecuteSqlAsync($"""
            IF OBJECT_ID(N'EskiIlerleme') IS NOT NULL
            BEGIN
                INSERT INTO KonuIlerlemeleri (KullaniciId, KonuId, TamamlanmaTarihi)
                SELECT {kullaniciId}, e.KayitId, e.Tarih FROM EskiIlerleme e
                WHERE e.Tur = 'Konu' AND EXISTS (SELECT 1 FROM Konular k WHERE k.Id = e.KayitId);

                INSERT INTO GorevIlerlemeleri (KullaniciId, GorevId, TamamlanmaTarihi)
                SELECT {kullaniciId}, e.KayitId, e.Tarih FROM EskiIlerleme e
                WHERE e.Tur = 'Gorev' AND EXISTS (SELECT 1 FROM Gorevler g WHERE g.Id = e.KayitId);

                INSERT INTO ProjeAdimIlerlemeleri (KullaniciId, ProjeAdimiId, TamamlanmaTarihi)
                SELECT {kullaniciId}, e.KayitId, e.Tarih FROM EskiIlerleme e
                WHERE e.Tur = 'ProjeAdimi' AND EXISTS (SELECT 1 FROM ProjeAdimlari p WHERE p.Id = e.KayitId);

                DROP TABLE EskiIlerleme;
            END
            """);

        if (tasinan > 0)
            logger.LogInformation("Eski ilerleme ilk kullanıcıya taşındı. KullaniciId={KullaniciId}, Satir={Satir}", kullaniciId, tasinan);
    }
}

/// <summary>
/// Kayıt/giriş sırasında kullanıcıya gösterilecek hatalar.
/// UygulamaHatasi'ndan türediği için GlobalHataYakalayici bunu otomatik olarak doğru HTTP cevabına çevirir;
/// controller'da try/catch gerekmez (Adım 6).
/// </summary>
public class HesapHatasi(string mesaj, int durumKodu) : UygulamaHatasi(mesaj, durumKodu);
