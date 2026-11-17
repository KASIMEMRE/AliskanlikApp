using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Dtos;

namespace YolHaritasi.Api.Controllers;

/// <summary>
/// CONTROLLER: HTTP isteklerini karşılayan sınıf.
/// [ApiController] → API davranışlarını açar (otomatik doğrulama, 400 cevapları vb.)
/// [Route("api/[controller]")] → [controller] yerine sınıf adı "Controller" eki olmadan gelir:
///    AlanlarController → /api/alanlar
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Alanlar")]   // API arayüzünde (Scalar) bu grubun başlığı
public class AlanlarController(UygulamaDbContext db) : ControllerBase
{
    /// <summary>Tüm alanlar ve her birindeki ilerleme yüzdesi.</summary>
    // GET /api/alanlar
    [HttpGet]
    public async Task<List<AlanOzetDto>> Listele()
    {
        // Bu sorgu SQL Server'da tek bir SELECT'e dönüşür; COUNT'lar alt sorgu olarak hesaplanır.
        // Tüm konuları belleğe çekip C#'ta saymaktan çok daha verimlidir.
        var alanlar = await db.Alanlar
            .AsNoTracking()
            .OrderBy(a => a.Sira)
            .Select(a => new
            {
                a.Id,
                a.Ad,
                a.Aciklama,
                Toplam = a.Konular.Count,
                Tamamlanan = a.Konular.Count(k => k.TamamlanmaTarihi != null),
            })
            .ToListAsync();

        // Yüzde hesabını C# tarafında yapıyoruz (basit matematik, SQL'e yüklemeye gerek yok).
        return alanlar
            .Select(a => new AlanOzetDto(a.Id, a.Ad, a.Aciklama, a.Toplam, a.Tamamlanan,
                a.Toplam == 0 ? 0 : a.Tamamlanan * 100 / a.Toplam))
            .ToList();
    }

    /// <summary>Bir alanın tüm konuları (sıralı).</summary>
    // GET /api/alanlar/2
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlanDetayDto>> Getir(int id)
    {
        var alan = await db.Alanlar
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AlanDetayDto(
                a.Id,
                a.Ad,
                a.Aciklama,
                a.Konular
                    .OrderBy(k => k.Sira)
                    .Select(k => new KonuOzetDto(k.Id, k.Sira, k.Baslik, k.Ozet, k.TahminiDakika, k.TamamlanmaTarihi != null))
                    .ToList()))
            .FirstOrDefaultAsync();

        // ActionResult<T>: Ya T (→ 200 OK) ya da başka bir HTTP sonucu (404, 400...) dönebilir.
        return alan is null ? NotFound(new HataDto($"{id} numaralı alan bulunamadı.")) : alan;
    }
}
