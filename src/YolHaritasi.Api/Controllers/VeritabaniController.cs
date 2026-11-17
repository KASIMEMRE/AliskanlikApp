using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YolHaritasi.Api.Entities;
using YolHaritasi.Api.Dtos;
using YolHaritasi.Api.Services;

namespace YolHaritasi.Api.Controllers;

/// <summary>Veritabanı bağlantı bilgisi, tablolar ve SQL pratik alanı.</summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Veritabanı")]
public class VeritabaniController(VeritabaniServisi servis, IWebHostEnvironment ortam) : ControllerBase
{
    /// <summary>Bağlantı durumu, SQL Server sürümü, uygulanan migration'lar, tablolar, sütunlar ve kayıt sayıları.</summary>
    // GET /api/veritabani/durum
    [HttpGet("durum")]
    public Task<VeritabaniDurumDto> Durum() => servis.DurumAsync();

    /// <summary>Sadece okuma (SELECT) sorgusu çalıştırır. Yalnızca geliştirme ortamında açıktır.</summary>
    // POST /api/veritabani/sorgu    gövde: { "sql": "SELECT * FROM Alanlar" }
    // Adım 4: Artık Kullanicilar tablosunda şifre hash'leri var. Herkes SQL çalıştırabilirse başkalarının
    // hash'lerini görebilir. Bu yüzden sadece Admin rolü (ilk kayıt olan kullanıcı) çalıştırabilir → diğerlerine 403.
    [Authorize(Roles = Roller.Admin)]
    [HttpPost("sorgu")]
    public async Task<ActionResult<SorguSonucuDto>> Sorgu([FromBody] SorguIstegi istek)
    {
        // Kullanıcının veritabanında SQL çalıştırabilmesi canlı (production) bir sunucuda çok tehlikelidir.
        // ASPNETCORE_ENVIRONMENT = Development değilse bu özellik kapalı.
        if (!ortam.IsDevelopment())
            return StatusCode(StatusCodes.Status403Forbidden,
                new HataDto("SQL pratik alanı sadece geliştirme ortamında kullanılabilir."));

        try
        {
            return await servis.SorguCalistirAsync(istek.Sql);
        }
        catch (SorguHatasi ex)
        {
            return BadRequest(new HataDto(ex.Message));   // 400
        }
    }
}
