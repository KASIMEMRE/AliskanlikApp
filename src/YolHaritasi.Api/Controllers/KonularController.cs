using Microsoft.AspNetCore.Mvc;
using YolHaritasi.Api.Auth;
using YolHaritasi.Api.Dtos;
using YolHaritasi.Api.Services;

namespace YolHaritasi.Api.Controllers;

/// <summary>
/// Bu controller veritabanını doğrudan bilmez; işi IIlerlemeServisi'ne devreder.
/// Controller "ince" kalır: isteği al → servise ver → sonucu uygun HTTP cevabına çevir.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Konular")]   // API arayüzünde (Scalar) bu grubun başlığı
public class KonularController(IIlerlemeServisi ilerleme) : ControllerBase
{
    /// <summary>Konunun detayı: terimler ve görevler.</summary>
    // GET /api/konular/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<KonuDetayDto>> Getir(int id)
    {
        var konu = await ilerleme.KonuDetayAsync(User.KullaniciId(), id);
        return konu is null ? NotFound(new HataDto($"{id} numaralı konu bulunamadı.")) : konu;
    }

    /// <summary>Konuyu tamamlandı olarak işaretler.</summary>
    // POST /api/konular/5/tamamla
    // Neden GET değil? GET isteği veriyi DEĞİŞTİRMEMELİ (safe method kuralı).
    // Tarayıcılar, arama motorları GET linklerini istemeden çağırabilir.
    [HttpPost("{id:int}/tamamla")]
    public async Task<IActionResult> Tamamla(int id)
    {
        var bulundu = await ilerleme.KonuyuTamamlaAsync(User.KullaniciId(), id);

        // 204 No Content: "İşlem başarılı, sana gönderecek bir gövdem yok."
        return bulundu ? NoContent() : NotFound(new HataDto($"{id} numaralı konu bulunamadı."));
    }
}
