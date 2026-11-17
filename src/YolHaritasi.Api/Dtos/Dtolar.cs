namespace YolHaritasi.Api.Dtos;

// ============================================================================
//  DTO (Data Transfer Object) = API'nin dışarıya gönderdiği veri şekli.
//
//  Neden entity'yi (Konu, Alan...) doğrudan döndürmüyoruz?
//   1) Entity'lerde döngüsel ilişkiler var (Konu → Alan → Konular → Konu ...).
//      JSON'a çevirirken sonsuz döngüye girer.
//   2) Veritabanı yapısını değiştirdiğimizde API'yi kullanan mobil uygulama bozulmasın.
//   3) Dışarıya sadece gerekeni göstermek (ileride örn. şifre hash'i asla dışarı çıkmamalı).
//
//  "record" kısa ve değiştirilemez (immutable) veri taşıyıcısıdır; DTO için idealdir.
// ============================================================================

public record AlanOzetDto(
    int Id,
    string Ad,
    string Aciklama,
    int KonuSayisi,
    int TamamlananKonuSayisi,
    int YuzdeIlerleme);

public record KonuOzetDto(
    int Id,
    int Sira,
    string Baslik,
    string Ozet,
    int TahminiDakika,
    bool Tamamlandi);

public record AlanDetayDto(
    int Id,
    string Ad,
    string Aciklama,
    List<KonuOzetDto> Konular);

public record TerimDto(int Id, string Ad, string Tanim, string? Ornek, string? Kullanim, string? KodOrnegi, string? KodDili);

public record GorevDto(int Id, int Sira, string Aciklama, bool Tamamlandi);

public record KaynakDto(string Baslik, string Url, string Site);

public record KonuDetayDto(
    int Id,
    string AlanAdi,
    int Sira,
    string Baslik,
    string Ozet,
    int TahminiDakika,
    DateTime? TamamlanmaTarihi,
    string? DersNotu,
    List<TerimDto> Terimler,
    List<GorevDto> Gorevler,
    List<KaynakDto> Kaynaklar);

/// <summary>Sözlük araması sonucu: terim + hangi konuya ait olduğu.</summary>
public record TerimAramaDto(int Id, string Ad, string Tanim, string? Ornek, string? Kullanim, string? KodOrnegi, string? KodDili,
    int KonuId, string KonuBaslik, string AlanAdi);

/// <summary>Hata durumunda dönen Türkçe mesaj. Örn: { "mesaj": "5 numaralı konu bulunamadı." }</summary>
public record HataDto(string Mesaj);

/// <summary>"Bugün ne yapmalıyım?" cevabı.</summary>
public record BugunDto(
    string Mesaj,
    int GenelYuzdeIlerleme,
    KonuDetayDto? Konu);

// ---- Projeler ----
public record ProjeAdimiDto(int Id, int Sira, string Aciklama, string? Ipucu, bool Tamamlandi);

public record ProjeDto(
    int Id,
    string AlanAdi,
    string Baslik,
    string Aciklama,
    string Zorluk,
    int AdimSayisi,
    int TamamlananAdimSayisi,
    List<ProjeAdimiDto> Adimlar);

// ---- Veritabanı ekranı ----
public record SutunDto(string Ad, string Tip, bool BosOlabilir, bool BirincilAnahtar, string? BagliTablo);

public record TabloDto(string Ad, long KayitSayisi, List<SutunDto> Sutunlar);

public record VeritabaniDurumDto(
    bool Bagli,
    string Saglayici,
    string Sunucu,
    string Veritabani,
    string? SqlServerSurumu,
    List<string> UygulananMigrationlar,
    List<TabloDto> Tablolar,
    string? Hata);

public record SorguIstegi(string Sql);

public record SorguSonucuDto(
    List<string> Sutunlar,
    List<List<string?>> Satirlar,
    int SatirSayisi,
    bool Kesildi,
    long SureMs);
