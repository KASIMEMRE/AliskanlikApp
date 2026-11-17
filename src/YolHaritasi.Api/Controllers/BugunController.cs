using Microsoft.AspNetCore.Mvc;
using YolHaritasi.Api.Dtos;
using YolHaritasi.Api.Services;

namespace YolHaritasi.Api.Controllers;

/// <summary>Uygulamanın ana ekranı: "Bugün ne yapmalıyım?"</summary>
[ApiController]
[Route("api/[controller]")]
public class BugunController(IIlerlemeServisi ilerleme) : ControllerBase
{
    /// <summary>Sıradaki konu, terimleri, görevleri ve genel ilerleme yüzdesi.</summary>
    // GET /api/bugun
    [HttpGet]
    public Task<BugunDto> Getir() => ilerleme.BugunAsync();
}
