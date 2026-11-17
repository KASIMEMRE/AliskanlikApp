using System.Security.Claims;

namespace YolHaritasi.Api.Auth;

/// <summary>
/// OPTIONS PATTERN: appsettings.json'daki "Jwt" bölümü bu sınıfa otomatik doldurulur.
/// Program.cs'te: builder.Services.Configure&lt;JwtAyarlari&gt;(builder.Configuration.GetSection("Jwt"));
/// Kullanırken: constructor'da IOptions&lt;JwtAyarlari&gt; istenir.
///
///   "Jwt": {
///     "Anahtar": "...en az 32 karakter gizli anahtar...",
///     "Yayinci": "YolHaritasi.Api",
///     "Hedef": "YolHaritasi.Istemci",
///     "SureDakika": 1440
///   }
/// </summary>
public class JwtAyarlari
{
    public const string Bolum = "Jwt";

    /// <summary>Token'ı İMZALAYAN gizli anahtar. Bunu bilen herkes geçerli token üretebilir; asla Git'e konmaz (geliştirme hariç).</summary>
    public string Anahtar { get; set; } = "";

    /// <summary>Issuer: Token'ı kim üretti? Doğrulamada kontrol edilir.</summary>
    public string Yayinci { get; set; } = "";

    /// <summary>Audience: Token kimin için üretildi? Doğrulamada kontrol edilir.</summary>
    public string Hedef { get; set; } = "";

    /// <summary>Token kaç dakika geçerli. Süresi dolunca kullanıcı tekrar giriş yapar.</summary>
    public int SureDakika { get; set; } = 1440;
}

/// <summary>
/// Token içindeki bilgilere (CLAIM) kolay erişim için EXTENSION METHOD'lar.
/// "this ClaimsPrincipal kullanici" → her controller'daki User nesnesinde User.KullaniciId() diye çağrılabilir.
/// </summary>
public static class KullaniciUzantilari
{
    // Token'a koyduğumuz claim adları (TokenServisi ile aynı olmalı)
    public const string IdClaim = "sub";     // subject: kullanıcının kimliği
    public const string AdClaim = "name";
    public const string RolClaim = "role";

    public static int KullaniciId(this ClaimsPrincipal kullanici) =>
        int.TryParse(kullanici.FindFirstValue(IdClaim), out var id)
            ? id
            : throw new InvalidOperationException("Token'da kullanıcı kimliği yok. Endpoint'te [Authorize] eksik olabilir.");
}
