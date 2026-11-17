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

    // ---- Adım 3: Gerçek kullanım örnekleri ----
    // Kullanim: Bu terim gerçek projelerde nerede / ne zaman karşına çıkar (Türkçe açıklama)
    // KodOrnegi: Çalışan, gerçekçi kod / SQL / komut örneği (çok satırlı)
    // KodDili: Örneğin dili: "csharp", "sql", "powershell", "http", "json", "text"
    public string? Kullanim { get; set; }
    public string? KodOrnegi { get; set; }
    public string? KodDili { get; set; }

    public int KonuId { get; set; }
    public Konu Konu { get; set; } = null!;
}
