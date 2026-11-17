using Microsoft.AspNetCore.Mvc;
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
                p.Adimlar.Count(a => a.Tamamlandi),
                p.Adimlar.OrderBy(a => a.Sira)
                    .Select(a => new ProjeAdimiDto(a.Id, a.Sira, a.Aciklama, a.Ipucu, a.Tamamlandi))
                    .ToList()))
            .ToListAsync();
    }

    public record AdimDurumIstegi(bool Tamamlandi);

    /// <summary>Bir proje adımını tamamlandı / tamamlanmadı olarak işaretler.</summary>
    // PATCH /api/projeler/adimlar/12    gövde: { "tamamlandi": true }
    [HttpPatch("adimlar/{id:int}")]
    public async Task<IActionResult> AdimDurumu(int id, [FromBody] AdimDurumIstegi istek)
    {
        // ExecuteUpdateAsync: Nesneyi önce belleğe çekmeden DOĞRUDAN tek bir UPDATE komutu gönderir.
        //   UPDATE ProjeAdimlari SET Tamamlandi = @p WHERE Id = @id
        // Dönüş değeri etkilenen satır sayısıdır. 0 ise o Id'de bir adım yok demektir.
        // (Karşılaştır: GorevlerController'daki yöntem önce FindAsync ile okur, sonra SaveChanges ile yazar.)
        var etkilenen = await db.ProjeAdimlari
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Tamamlandi, istek.Tamamlandi));

        return etkilenen == 0
            ? NotFound(new HataDto($"{id} numaralı proje adımı bulunamadı."))
            : NoContent();
    }
}
