namespace YolHaritasi.Api.Entities;

/// <summary>
/// Gorev = Bir konuyu öğrenirken yapılacak somut iş. Örn: "F12 Network sekmesinde bir isteği incele".
/// Tablo adı: Gorevler
/// </summary>
public class Gorev
{
    public int Id { get; set; }
    public string Aciklama { get; set; } = "";
    public int Sira { get; set; }
    public bool Tamamlandi { get; set; }   // SQL Server'da "bit" tipi olur (0/1)

    public int KonuId { get; set; }
    public Konu Konu { get; set; } = null!;
}
