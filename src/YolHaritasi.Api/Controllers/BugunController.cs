using Microsoft.AspNetCore.Mvc;
using YolHaritasi.Api.Auth;
using YolHaritasi.Api.Dtos;
using YolHaritasi.Api.Services;

namespace YolHaritasi.Api.Controllers;

/// <summary>Uygulamanın ana ekranı: "Bugün ne yapmalıyım?"</summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Bugün")]   // API arayüzünde (Scalar) bu grubun başlığı
public class BugunController(IIlerlemeServisi ilerleme) : ControllerBase
{
    /// <summary>Sıradaki konu, terimleri, görevleri ve genel ilerleme yüzdesi.</summary>
    // GET /api/bugun
    [HttpGet]
    public Task<BugunDto> Getir() => ilerleme.BugunAsync(User.KullaniciId());   // User.KullaniciId(): token'daki kullanıcı
}
