using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Auth;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Dtos;
using YolHaritasi.Api.Entities;
using YolHaritasi.Api.Services;

namespace YolHaritasi.Api.Controllers;

/// <summary>Çalışma süresi kaydı ve istatistikler.</summary>
[ApiController]
[Tags("Çalışma ve İstatistik")]
public class CalismaController(UygulamaDbContext db, IstatistikServisi istatistik) : ControllerBase
{
    /// <summary>Son N gündeki çalışma oturumlarım (varsayılan 30 gün).</summary>
    // GET /api/calisma?gun=30
    [HttpGet("api/calisma")]
    public async Task<List<CalismaDto>> Listele([FromQuery] int gun = 30)
    {
        gun = Math.Clamp(gun, 1, 365);   // aşırı büyük değerle tüm tabloyu çekmeyi engelle
        var kullaniciId = User.KullaniciId();
        var sinir = DateTime.UtcNow.AddDays(-gun);

        return await db.CalismaOturumlari
            .AsNoTracking()
            .Where(c => c.KullaniciId == kullaniciId && c.Baslangic >= sinir)
            .OrderByDescending(c => c.Baslangic)
            .Select(c => new CalismaDto(c.Id, c.KonuId, c.Konu.Baslik, c.Baslangic, c.Dakika, c.Aciklama))
            .ToListAsync();
    }

    /// <summary>Çalışma oturumu kaydeder (zamanlayıcı bitince veya elle).</summary>
    // POST /api/calisma    gövde: { "konuId": 5, "dakika": 45, "aciklama": "Ders notunu okudum" }
    [HttpPost("api/calisma")]
    public async Task<ActionResult<CalismaDto>> Ekle(CalismaIstegi istek)
    {
        // DataAnnotations temel kuralları kontrol etti (dakika 1-600). İş kuralları burada:
        var konu = await db.Konular.AsNoTracking().SingleOrDefaultAsync(k => k.Id == istek.KonuId);
        if (konu is null)
            return NotFound(new HataDto("Konu bulunamadı."));

        var baslangic = istek.Baslangic?.ToUniversalTime() ?? DateTime.UtcNow.AddMinutes(-istek.Dakika);
        if (baslangic > DateTime.UtcNow.AddMinutes(5))
            return BadRequest(new HataDto("Gelecek tarihli çalışma kaydedilemez."));

        var oturum = new CalismaOturumu
        {
            KullaniciId = User.KullaniciId(),
            KonuId = istek.KonuId,
            Dakika = istek.Dakika,
            Aciklama = string.IsNullOrWhiteSpace(istek.Aciklama) ? null : istek.Aciklama.Trim(),
            Baslangic = baslangic,
        };
        db.CalismaOturumlari.Add(oturum);
        await db.SaveChangesAsync();

        return Created("/api/calisma", new CalismaDto(oturum.Id, konu.Id, konu.Baslik, oturum.Baslangic, oturum.Dakika, oturum.Aciklama));
    }

    /// <summary>Çalışma kaydını siler (sadece kendi kaydın).</summary>
    // DELETE /api/calisma/7
    [HttpDelete("api/calisma/{id:int}")]
    public async Task<IActionResult> Sil(int id)
    {
        var kullaniciId = User.KullaniciId();
        var silinen = await db.CalismaOturumlari.Where(c => c.Id == id && c.KullaniciId == kullaniciId).ExecuteDeleteAsync();
        return silinen == 0 ? NotFound(new HataDto("Kayıt bulunamadı.")) : NoContent();
    }

    /// <summary>İstatistiklerim: toplam süre, seri, son 14 gün, alan bazında dağılım.</summary>
    // GET /api/istatistik
    [HttpGet("api/istatistik")]
    public Task<IstatistikDto> Istatistik() => istatistik.GetirAsync(User.KullaniciId());
}
