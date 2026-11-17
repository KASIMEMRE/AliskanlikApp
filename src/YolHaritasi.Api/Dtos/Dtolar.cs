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

// ---- Hesap (Adım 4) ----
// DATA ANNOTATIONS ile DOĞRULAMA (validation): [ApiController] olan controller'larda,
// kurallara uymayan istek metoda HİÇ ULAŞMADAN otomatik 400 Bad Request ile reddedilir.
// ErrorMessage: Kullanıcıya gösterilecek Türkçe hata mesajı.
// Record'larda attribute'lar CONSTRUCTOR PARAMETRESİNE yazılır. ("[property: ...]" yazarsan ASP.NET Core
// doğrulamayı yok sayılacak diye hata verir; bunu bizzat yaşadık, docs/adim-04.md'ye bak.)

public record KayitIstegi(
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Ad zorunludur.")]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 2, ErrorMessage = "Ad 2-100 karakter olmalıdır.")]
    string Ad,

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "E-posta zorunludur.")]
    [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    string Email,

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Şifre zorunludur.")]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
    string Sifre);

public record GirisIstegi(
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "E-posta zorunludur.")]
    string Email,

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Şifre zorunludur.")]
    string Sifre);

public record KullaniciDto(int Id, string Ad, string Email, string Rol);

/// <summary>Giriş/kayıt cevabı: token + ne zaman geçersiz olacağı + kullanıcı bilgisi.</summary>
public record OturumDto(string Token, DateTime BitisTarihi, KullaniciDto Kullanici);

// ---- Notlar ve çalışma süresi (Adım 5) ----
public record NotDto(int Id, int KonuId, string Icerik, DateTime OlusturmaTarihi, DateTime? GuncellemeTarihi);

public record NotIstegi(
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Not boş olamaz.")]
    [System.ComponentModel.DataAnnotations.StringLength(4000, ErrorMessage = "Not en fazla 4000 karakter olabilir.")]
    string Icerik);

public record CalismaDto(int Id, int KonuId, string KonuBaslik, DateTime Baslangic, int Dakika, string? Aciklama);

public record CalismaIstegi(
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Geçerli bir konu seçin.")]
    int KonuId,

    [System.ComponentModel.DataAnnotations.Range(1, 600, ErrorMessage = "Süre 1 ile 600 dakika arasında olmalıdır.")]
    int Dakika,

    [System.ComponentModel.DataAnnotations.StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    string? Aciklama,

    // Boşsa "şimdi" kabul edilir (zamanlayıcı bittiğinde)
    DateTime? Baslangic);

// ---- İstatistik ----
public record GunlukDakikaDto(DateOnly Gun, int Dakika);
public record AlanIstatistikDto(string Alan, int Dakika, int TamamlananKonu, int ToplamKonu);

public record IstatistikDto(
    int ToplamDakika,
    int BuHaftaDakika,
    int TamamlananKonu,
    int ToplamKonu,
    int NotSayisi,
    int GuncelSeri,        // bugün (veya dün) dahil arka arkaya çalışılan gün sayısı
    int EnUzunSeri,
    List<GunlukDakikaDto> Son14Gun,
    List<AlanIstatistikDto> Alanlar);
