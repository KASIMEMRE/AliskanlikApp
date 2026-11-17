using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace YolHaritasi.Api.Controllers;

/// <summary>
/// SADECE GELİŞTİRME ORTAMINDA: Hata yönetimini ve loglamayı kendi gözünle görmek için deneme endpoint'leri.
/// Canlı ortamda 404 döner.
/// </summary>
[ApiController]
[Route("api/gelistirici")]
[Tags("Geliştirici (sadece Development)")]
[AllowAnonymous]
public class GelistiriciController(IWebHostEnvironment ortam, ILogger<GelistiriciController> logger) : ControllerBase
{
    /// <summary>Bilerek beklenmeyen bir hata fırlatır → 500 + ProblemDetails + trace id. Sonra logda trace id'yi ara.</summary>
    // GET /api/gelistirici/hata
    [HttpGet("hata")]
    public IActionResult Hata()
    {
        if (!ortam.IsDevelopment()) return NotFound();

        logger.LogInformation("Bilerek hata fırlatılacak (deneme)");
        int[] liste = [1, 2, 3];
        var olmayan = liste[10];   // IndexOutOfRangeException → GlobalHataYakalayici → 500
        return Ok(olmayan);
    }

    /// <summary>Tüm log seviyelerinde birer satır yazar. Konsolda ve logs/ klasöründeki dosyada gör.</summary>
    // GET /api/gelistirici/log
    [HttpGet("log")]
    public IActionResult Log()
    {
        if (!ortam.IsDevelopment()) return NotFound();

        logger.LogTrace("Trace: en ayrıntılı seviye (genelde kapalı)");
        logger.LogDebug("Debug: geliştirme sırasında işe yarar ayrıntı");
        logger.LogInformation("Information: normal akış. Kullanıcı={Kullanici}, Saat={Saat}", "deneme", DateTime.Now);
        logger.LogWarning("Warning: beklenmeyen ama uygulamayı durdurmayan durum");
        logger.LogError("Error: bir işlem başarısız oldu");
        logger.LogCritical("Critical: uygulama ciddi tehlikede");
        return Ok(new { mesaj = "6 seviyede log yazıldı. appsettings'teki MinimumLevel'a göre bazıları görünmez." });
    }
}
