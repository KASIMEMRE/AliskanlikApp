namespace YolHaritasi.Api.Entities;

/// <summary>
/// Terim = Sözlükteki bir kavram. Örn: "Endpoint", "Primary Key", "Token".
/// Tablo adı: Terimler
/// </summary>
public class Terim
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Tanim { get; set; } = "";

    // Örnek zorunlu değil, bu yüzden "string?" (nullable). Veritabanında NULL olabilir.
    public string? Ornek { get; set; }

    public int KonuId { get; set; }
    public Konu Konu { get; set; } = null!;
}
