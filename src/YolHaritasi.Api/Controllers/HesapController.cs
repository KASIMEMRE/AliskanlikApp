using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YolHaritasi.Api.Auth;
using YolHaritasi.Api.Dtos;
using YolHaritasi.Api.Services;

namespace YolHaritasi.Api.Controllers;

/// <summary>Kayıt, giriş ve oturumdaki kullanıcı.</summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Hesap")]
public class HesapController(HesapServisi hesap) : ControllerBase
{
    /// <summary>Yeni hesap oluşturur ve giriş yapmış olarak token döner. İlk kayıt olan kullanıcı Admin olur.</summary>
    // POST /api/hesap/kayit   gövde: { "ad": "Emre", "email": "emre@ornek.com", "sifre": "en-az-8-karakter" }
    [AllowAnonymous]   // Giriş yapmamış kişi de çağırabilmeli (yoksa kimse kayıt olamaz!)
    [HttpPost("kayit")]
    public async Task<ActionResult<OturumDto>> Kayit(KayitIstegi istek)
    {
        try
        {
            return await hesap.KayitAsync(istek);
        }
        catch (HesapHatasi ex)
        {
            return Conflict(new HataDto(ex.Message));   // 409 Conflict: e-posta zaten kayıtlı
        }
    }

    /// <summary>E-posta ve şifreyle giriş yapar, token döner.</summary>
    // POST /api/hesap/giris   gövde: { "email": "...", "sifre": "..." }
    [AllowAnonymous]
    [HttpPost("giris")]
    public async Task<ActionResult<OturumDto>> Giris(GirisIstegi istek)
    {
        try
        {
            return await hesap.GirisAsync(istek);
        }
        catch (HesapHatasi ex)
        {
            return Unauthorized(new HataDto(ex.Message));   // 401
        }
    }

    /// <summary>Token'ın sahibi olan kullanıcıyı döner. Token geçersizse 401.</summary>
    // GET /api/hesap/ben    başlık: Authorization: Bearer eyJ...
    [HttpGet("ben")]
    public async Task<ActionResult<KullaniciDto>> Ben()
    {
        // User: ASP.NET Core'un token'dan çıkardığı kullanıcı bilgisi (ClaimsPrincipal)
        var kullanici = await hesap.GetirAsync(User.KullaniciId());
        return kullanici is null ? Unauthorized() : kullanici;
    }
}
