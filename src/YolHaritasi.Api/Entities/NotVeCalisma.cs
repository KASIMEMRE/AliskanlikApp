namespace YolHaritasi.Api.Entities;

/// <summary>
/// Not = Kullanıcının bir konu hakkında aldığı kişisel not. Tablo: Notlar
/// Her not bir kullanıcıya AİTTİR: Başkasının notunu okuyamaz, değiştiremez, silemez.
/// (Bu kontrolü controller'da yapıyoruz → "sahiplik kontrolü / ownership check")
/// </summary>
public class Not
{
    public int Id { get; set; }
    public string Icerik { get; set; } = "";
    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellemeTarihi { get; set; }

    public int KullaniciId { get; set; }
    public Kullanici Kullanici { get; set; } = null!;

    public int KonuId { get; set; }
    public Konu Konu { get; set; } = null!;
}

/// <summary>
/// CalismaOturumu = Kullanıcının bir konu üzerinde çalıştığı süre. Tablo: CalismaOturumlari
/// Zamanlayıcıyla (Başlat/Bitir) veya elle girilir. İstatistik ekranı bu tablodan hesaplanır.
/// </summary>
public class CalismaOturumu
{
    public int Id { get; set; }
    public DateTime Baslangic { get; set; }
    public int Dakika { get; set; }
    public string? Aciklama { get; set; }

    public int KullaniciId { get; set; }
    public Kullanici Kullanici { get; set; } = null!;

    public int KonuId { get; set; }
    public Konu Konu { get; set; } = null!;
}
