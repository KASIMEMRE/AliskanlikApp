namespace YolHaritasi.Api.Entities;

/// <summary>
/// Konu = Bir alanın içindeki tek bir öğrenme adımı. Örn: "HTTP ve REST", "JOIN'ler".
/// Tablo adı: Konular
/// </summary>
public class Konu
{
    public int Id { get; set; }
    public string Baslik { get; set; } = "";
    public string Ozet { get; set; } = "";

    // Alan içindeki sırası (1, 2, 3...)
    public int Sira { get; set; }

    // DERS NOTU: Konunun Türkçe anlatımı (Markdown biçiminde: başlıklar, listeler, kod blokları).
    // Uzun metin olduğu için SQL Server'da nvarchar(max) olur. Yazılmamışsa NULL.
    public string? DersNotu { get; set; }

    // Tahmini çalışma süresi (dakika)
    public int TahminiDakika { get; set; }

    // İlerleme bilgisi. Şimdilik tek kullanıcı var; kullanıcı sistemi gelince ayrı tabloya taşınacak.
    // "DateTime?" sonundaki ? = NULLABLE: değer olmayabilir. Tamamlanmadıysa veritabanında NULL durur.
    public DateTime? TamamlanmaTarihi { get; set; }

    // FOREIGN KEY (yabancı anahtar): Bu konunun hangi Alan'a ait olduğunu tutan sütun.
    // Alanlar tablosundaki Id'ye işaret eder. Olmayan bir AlanId yazmaya çalışırsan SQL Server hata verir;
    // buna REFERENTIAL INTEGRITY (bilgi bütünlüğü) denir.
    public int AlanId { get; set; }

    // Navigation property'ler
    public Alan Alan { get; set; } = null!;
    public List<Terim> Terimler { get; set; } = [];
    public List<Gorev> Gorevler { get; set; } = [];
    public List<Kaynak> Kaynaklar { get; set; } = [];
}
