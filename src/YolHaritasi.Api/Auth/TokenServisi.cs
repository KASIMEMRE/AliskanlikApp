using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using YolHaritasi.Api.Entities;

namespace YolHaritasi.Api.Auth;

/// <summary>
/// JWT (JSON Web Token) üretir.
///
/// Bir JWT üç parçadır, aralarında nokta vardır:  BAŞLIK.İÇERİK.İMZA
///   Başlık : {"alg":"HS256","typ":"JWT"}                   → hangi algoritmayla imzalandı
///   İçerik : {"sub":"1","name":"Emre","role":"Admin","exp":...}  → CLAIM'ler (kullanıcı bilgisi)
///   İmza   : HMACSHA256(başlık + içerik, gizliAnahtar)       → değiştirilmediğinin kanıtı
///
/// ⚠️ İçerik ŞİFRELİ DEĞİLDİR, sadece Base64 ile kodlanmıştır; jwt.io'da herkes okuyabilir.
///    Bu yüzden içine şifre, TC no gibi hassas bilgi konmaz. Güvenlik imzadan gelir:
///    biri içeriği değiştirirse imza tutmaz ve sunucu token'ı reddeder.
/// </summary>
public class TokenServisi(IOptions<JwtAyarlari> ayarlar)
{
    private readonly JwtAyarlari _ayar = ayarlar.Value;

    public (string Token, DateTime BitisTarihi) Uret(Kullanici kullanici)
    {
        var bitis = DateTime.UtcNow.AddMinutes(_ayar.SureDakika);

        var tanim = new SecurityTokenDescriptor
        {
            Issuer = _ayar.Yayinci,
            Audience = _ayar.Hedef,
            Expires = bitis,
            Subject = new ClaimsIdentity(
            [
                new Claim(KullaniciUzantilari.IdClaim, kullanici.Id.ToString()),
                new Claim(KullaniciUzantilari.AdClaim, kullanici.Ad),
                new Claim(KullaniciUzantilari.RolClaim, kullanici.Rol),
                new Claim("email", kullanici.Email),
            ]),
            // Simetrik anahtar: aynı anahtar hem imzalar hem doğrular (HMAC-SHA256)
            SigningCredentials = new SigningCredentials(ImzaAnahtari(_ayar), SecurityAlgorithms.HmacSha256),
        };

        return (new JsonWebTokenHandler().CreateToken(tanim), bitis);
    }

    /// <summary>Program.cs'teki doğrulama ayarı da aynı anahtarı kullanır; bu yüzden ortak metot.</summary>
    public static SymmetricSecurityKey ImzaAnahtari(JwtAyarlari ayar) =>
        new(Encoding.UTF8.GetBytes(ayar.Anahtar));
}
