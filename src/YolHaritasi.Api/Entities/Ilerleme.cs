namespace YolHaritasi.Api.Entities;

// ============================================================================
//  KİŞİSEL İLERLEME TABLOLARI (Adım 4)
//
//  Önceden "tamamlandı" bilgisi doğrudan Konular / Gorevler / ProjeAdimlari tablolarında
//  duruyordu. Bu yüzden tek bir kişinin ilerlemesi tutulabiliyordu.
//  Artık her kullanıcının kendi ilerlemesi var. Bu bir ÇOKA ÇOK (many-to-many) ilişkidir:
//    Bir kullanıcı birçok konuyu tamamlar, bir konu birçok kullanıcı tarafından tamamlanır.
//  Araya bir BAĞLANTI TABLOSU (junction table) koyuyoruz. Tablonun kendi bilgisi de var
//  (TamamlanmaTarihi); buna "payload'lı çoka çok ilişki" denir.
//
//  Primary key iki sütundan oluşur: (KullaniciId, KonuId) → COMPOSITE (bileşik) KEY.
//  Böylece aynı kullanıcı aynı konuyu iki kez "tamamlayamaz"; veritabanı bunu engeller.
// ============================================================================

/// <summary>Kullanıcının tamamladığı konu. Tablo: KonuIlerlemeleri</summary>
public class KonuIlerleme
{
    public int KullaniciId { get; set; }
    public Kullanici Kullanici { get; set; } = null!;

    public int KonuId { get; set; }
    public Konu Konu { get; set; } = null!;

    public DateTime TamamlanmaTarihi { get; set; } = DateTime.UtcNow;
}

/// <summary>Kullanıcının yaptığı görev. Tablo: GorevIlerlemeleri</summary>
public class GorevIlerleme
{
    public int KullaniciId { get; set; }
    public Kullanici Kullanici { get; set; } = null!;

    public int GorevId { get; set; }
    public Gorev Gorev { get; set; } = null!;

    public DateTime TamamlanmaTarihi { get; set; } = DateTime.UtcNow;
}

/// <summary>Kullanıcının bitirdiği proje adımı. Tablo: ProjeAdimIlerlemeleri</summary>
public class ProjeAdimIlerleme
{
    public int KullaniciId { get; set; }
    public Kullanici Kullanici { get; set; } = null!;

    public int ProjeAdimiId { get; set; }
    public ProjeAdimi ProjeAdimi { get; set; } = null!;

    public DateTime TamamlanmaTarihi { get; set; } = DateTime.UtcNow;
}
