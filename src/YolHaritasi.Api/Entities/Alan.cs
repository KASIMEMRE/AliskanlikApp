namespace YolHaritasi.Api.Entities;

/// <summary>
/// ENTITY (varlık): Veritabanında bir TABLOYA karşılık gelen C# sınıfı.
/// Bu sınıfın her özelliği (property) tabloda bir SÜTUN olur,
/// her nesnesi (new Alan()) ise tabloda bir SATIR olur.
///
/// Alan = Öğrenme alanı. Örn: "Backend", "SQL", "Yapay Zeka", "Sunucu".
/// Tablo adı: Alanlar
/// </summary>
public class Alan
{
    // "Id" adlı int özellik, EF Core tarafından otomatik olarak PRIMARY KEY (birincil anahtar) yapılır.
    // SQL Server'da IDENTITY olur: her yeni satırda 1, 2, 3... diye kendisi artar.
    public int Id { get; set; }

    // string özellikler SQL Server'da nvarchar olur. Uzunluk sınırını DbContext'te veriyoruz.
    public string Ad { get; set; } = "";
    public string Aciklama { get; set; } = "";

    // Ekranda hangi sırayla gösterileceği
    public int Sira { get; set; }

    // NAVIGATION PROPERTY (gezinme özelliği): Tabloda sütun DEĞİLDİR.
    // "Bir Alan'ın birden çok Konu'su vardır" ilişkisini (1'e çok / one-to-many) C# tarafında ifade eder.
    public List<Konu> Konular { get; set; } = [];
}
