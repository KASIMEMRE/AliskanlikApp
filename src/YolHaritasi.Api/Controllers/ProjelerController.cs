using Microsoft.AspNetCore.Mvc;
using YolHaritasi.Api.Auth;
using YolHaritasi.Api.Entities;
using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Dtos;

namespace YolHaritasi.Api.Controllers;

/// <summary>Proje görevleri.</summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Projeler")]
public class ProjelerController(UygulamaDbContext db) : ControllerBase
{
    /// <summary>Tüm projeler, adımları ve ilerlemeleri (alan sırasına göre).</summary>
    // GET /api/projeler
    [HttpGet]
    public async Task<List<ProjeDto>> Listele()
    {
        var kullaniciId = User.KullaniciId();
        return await db.Projeler
            .AsNoTracking()
            .OrderBy(p => p.Alan.Sira).ThenBy(p => p.Sira)
            .Select(p => new ProjeDto(
                p.Id,
                p.Alan.Ad,
                p.Baslik,
                p.Aciklama,
                p.Zorluk,
                p.Adimlar.Count,
                p.Adimlar.Count(a => a.Ilerlemeler.Any(i => i.KullaniciId == kullaniciId)),
                p.Adimlar.OrderBy(a => a.Sira)
                    .Select(a => new ProjeAdimiDto(a.Id, a.Sira, a.Aciklama, a.Ipucu, a.Ilerlemeler.Any(i => i.KullaniciId == kullaniciId)))
                    .ToList()))
            .ToListAsync();
    }

    public record AdimDurumIstegi(bool Tamamlandi);

    /// <summary>Bir proje adımını tamamlandı / tamamlanmadı olarak işaretler (giriş yapan kullanıcı için).</summary>
    // PATCH /api/projeler/adimlar/12    gövde: { "tamamlandi": true }
    [HttpPatch("adimlar/{id:int}")]
    public async Task<IActionResult> AdimDurumu(int id, [FromBody] AdimDurumIstegi istek)
    {
        var kullaniciId = User.KullaniciId();
        if (!await db.ProjeAdimlari.AnyAsync(a => a.Id == id))
            return NotFound(new HataDto($"{id} numaralı proje adımı bulunamadı."));

        if (istek.Tamamlandi)
        {
            if (!await db.ProjeAdimIlerlemeleri.AnyAsync(i => i.KullaniciId == kullaniciId && i.ProjeAdimiId == id))
            {
                db.ProjeAdimIlerlemeleri.Add(new ProjeAdimIlerleme { KullaniciId = kullaniciId, ProjeAdimiId = id });
                await db.SaveChangesAsync();
            }
        }
        else
        {
            // ExecuteDeleteAsync: Nesneyi belleğe çekmeden DOĞRUDAN tek bir DELETE komutu gönderir.
            //   DELETE FROM ProjeAdimIlerlemeleri WHERE KullaniciId = @k AND ProjeAdimiId = @id
            await db.ProjeAdimIlerlemeleri
                .Where(i => i.KullaniciId == kullaniciId && i.ProjeAdimiId == id)
                .ExecuteDeleteAsync();
        }

        return NoContent();
    }
}
