namespace YolHaritasi.Api.Entities;

/// <summary>
/// Kaynak = Bir konuyu çalışırken okunacak dış ders sayfası (W3Schools, Microsoft Learn...).
/// Tablo adı: Kaynaklar
/// İlişki: Bir Konu'nun çok Kaynak'ı olur (one-to-many).
/// </summary>
public class Kaynak
{
    public int Id { get; set; }
    public string Baslik { get; set; } = "";
    public string Url { get; set; } = "";
    public string Site { get; set; } = "";   // "W3Schools", "Microsoft Learn"
    public int Sira { get; set; }

    public int KonuId { get; set; }
    public Konu Konu { get; set; } = null!;
}
