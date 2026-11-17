// ============================================================================
//  GÜN 1 — İlk Web API'miz
//  Bu dosya uygulamanın BAŞLANGIÇ NOKTASIDIR (entry point).
//  Program çalıştırıldığında kod yukarıdan aşağıya doğru bir kez çalışır,
//  en sondaki app.Run() satırında ise sunucu ayağa kalkar ve istek beklemeye başlar.
// ============================================================================

// 1) BUILDER (inşa edici)
//    Uygulamayı kurmadan önce ayarları ve servisleri topladığımız nesne.
//    "args" = komut satırından gelen parametreler (ör. --urls http://localhost:5000)
var builder = WebApplication.CreateBuilder(args);

// (İleride buraya builder.Services.Add...(...) satırları gelecek.
//  Bu bölüme "servis kaydı" / Dependency Injection denir. Gün 4'te göreceğiz.)

// 2) APP (uygulama)
//    Ayarlar bitti, artık çalışacak uygulama nesnesini üretiyoruz.
var app = builder.Build();

// 3) VERİ — şimdilik bellekte (RAM'de) tutulan basit bir liste.
//    Uygulama kapanınca bu veriler SİLİNİR. Kalıcı veri için ileride veritabanı kullanacağız.
var aliskanliklar = new List<Aliskanlik>
{
    new(1, "Su iç", "Günde 8 bardak", 0),
    new(2, "Kitap oku", "Günde 20 sayfa", 3),
    new(3, "Yürüyüş", "30 dakika", 1),
};

// 4) ENDPOINT'LER (uç noktalar)
//    Her biri şu soruyu cevaplar: "Şu ADRESE, şu METOTLA bir istek gelirse ne yapayım?"
//
//    app.MapGet( "adres" , çalışacak_fonksiyon )
//            │      │            │
//            │      │            └─ İstek gelince çalışan kod (lambda: () => ...)
//            │      └─ ROUTE (yol): URL'nin alan adından sonraki kısmı
//            └─ HTTP METODU: GET = "veri getir / oku"

// GET /  → Sunucu ayakta mı diye kontrol etmek için basit bir cevap
app.MapGet("/", () => "Alışkanlık API çalışıyor!");

// GET /aliskanliklar  → Tüm alışkanlıkları JSON olarak döndürür.
//    C# listesini biz JSON'a çevirmiyoruz; ASP.NET Core bunu otomatik yapar
//    (bu işleme SERIALIZATION / serileştirme denir).
app.MapGet("/aliskanliklar", () => aliskanliklar);

// GET /aliskanliklar/2  → Tek bir alışkanlık getirir.
//    {id:int} bir ROUTE PARAMETRESİDİR: URL'deki "2" değeri otomatik olarak
//    aşağıdaki "int id" parametresine aktarılır (buna MODEL BINDING denir).
//    ":int" bir kısıttır; /aliskanliklar/abc gibi bir istek bu endpoint'e hiç düşmez.
app.MapGet("/aliskanliklar/{id:int}", (int id) =>
{
    var bulunan = aliskanliklar.FirstOrDefault(a => a.Id == id);

    // Bulunamadıysa 404 Not Found, bulunduysa 200 OK + veri döndürüyoruz.
    // Bu sayılara HTTP DURUM KODU (status code) denir.
    return bulunan is null
        ? Results.NotFound(new { mesaj = $"{id} numaralı alışkanlık bulunamadı" })
        : Results.Ok(bulunan);
});

// 5) ÇALIŞTIR — Sunucu bir PORT'u dinlemeye başlar ve program burada "bekler".
//    Ctrl+C'ye basana kadar gelen istekleri karşılar.
app.Run();


// ============================================================================
//  MODEL — verimizin şekli
//  "record" = sadece veri taşımak için tasarlanmış kısa sınıf tanımı.
//  Aşağıdaki tek satır; Id, Ad, Aciklama, Seri özelliklerine sahip bir tip oluşturur.
//  Seri (streak) = alışkanlığın kaç gündür aralıksız yapıldığı.
// ============================================================================
record Aliskanlik(int Id, string Ad, string Aciklama, int Seri);
