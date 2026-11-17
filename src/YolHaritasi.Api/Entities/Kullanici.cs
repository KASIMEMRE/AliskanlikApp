namespace YolHaritasi.Api.Entities;

/// <summary>
/// Kullanici = Uygulamaya kayıt olan kişi. Tablo adı: Kullanicilar
/// </summary>
public class Kullanici
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";

    // E-posta giriş için kullanılır; aynı e-postayla iki hesap olamaz (UNIQUE index, DbContext'te).
    public string Email { get; set; } = "";

    // ŞİFRE ASLA DÜZ METİN SAKLANMAZ. Sadece hash'i saklanır.
    // Hash tek yönlüdür: "AQAAAAIAAYagAAAAE..." metninden şifreye geri dönülemez.
    public string SifreHash { get; set; } = "";

    // Yetkilendirme (authorization) için rol: "Kullanici" veya "Admin"
    public string Rol { get; set; } = Roller.Kullanici;

    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

    // Bu kullanıcının ilerleme kayıtları
    public List<KonuIlerleme> KonuIlerlemeleri { get; set; } = [];
}

/// <summary>Rol adları. Metni her yere elle yazıp yazım hatası yapmamak için sabit (const) kullanıyoruz.</summary>
public static class Roller
{
    public const string Kullanici = "Kullanici";
    public const string Admin = "Admin";
}
