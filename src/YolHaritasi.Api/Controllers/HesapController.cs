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
        // Adım 6: try/catch yok. E-posta kayıtlıysa servis HesapHatasi(409) fırlatır;
        // GlobalHataYakalayici bunu 409 Conflict + ProblemDetails cevabına çevirir.
        => await hesap.KayitAsync(istek);

    /// <summary>E-posta ve şifreyle giriş yapar, token döner.</summary>
    // POST /api/hesap/giris   gövde: { "email": "...", "sifre": "..." }
    [AllowAnonymous]
    [HttpPost("giris")]
    public async Task<ActionResult<OturumDto>> Giris(GirisIstegi istek)
        => await hesap.GirisAsync(istek);   // hatalı şifre → HesapHatasi(401) → GlobalHataYakalayici

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
