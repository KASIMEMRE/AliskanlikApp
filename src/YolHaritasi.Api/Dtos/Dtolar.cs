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

public record TerimDto(int Id, string Ad, string Tanim, string? Ornek);

public record GorevDto(int Id, int Sira, string Aciklama, bool Tamamlandi);

public record KonuDetayDto(
    int Id,
    string AlanAdi,
    int Sira,
    string Baslik,
    string Ozet,
    int TahminiDakika,
    DateTime? TamamlanmaTarihi,
    List<TerimDto> Terimler,
    List<GorevDto> Gorevler);

/// <summary>Sözlük araması sonucu: terim + hangi konuya ait olduğu.</summary>
public record TerimAramaDto(int Id, string Ad, string Tanim, string? Ornek, int KonuId, string KonuBaslik, string AlanAdi);

/// <summary>Hata durumunda dönen Türkçe mesaj. Örn: { "mesaj": "5 numaralı konu bulunamadı." }</summary>
public record HataDto(string Mesaj);

/// <summary>"Bugün ne yapmalıyım?" cevabı.</summary>
public record BugunDto(
    string Mesaj,
    int GenelYuzdeIlerleme,
    KonuDetayDto? Konu);
