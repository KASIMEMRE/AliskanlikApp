using YolHaritasi.Api.Entities;

namespace YolHaritasi.Api.Data;

// ============================================================================
//  PROJE GÖREVLERİ: Her alanda öğrendiklerini uygulayacağın adım adım projeler.
//  Anahtar: Alan adı
// ============================================================================
public static partial class BaslangicVerisi
{
    private static readonly Dictionary<string, Proje[]> ProjeListesi = new()
    {
        ["C# Temelleri"] =
        [
            P(1, "Konsol Görev Yöneticisi", "Kolay",
                "Komut satırında çalışan, görev ekleyip listeleyen ve dosyaya kaydeden bir uygulama. Değişkenler, döngüler, List, sınıflar ve dosya işlemlerini bir arada kullanırsın.",
                [
                    ("dotnet new console -n GorevYoneticisi ile yeni bir konsol projesi oluştur.", null),
                    ("Id, Baslik, TamamlandiMi ve OlusturmaTarihi property'leri olan bir Gorev sınıfı yaz.", "C# Temelleri → 7. konu: Sınıf ve Nesne"),
                    ("while döngüsü içinde menü göster: 1) Ekle 2) Listele 3) Tamamla 4) Sil 5) Çıkış. Seçimi switch ile işle.", "C# Temelleri → 4. konu"),
                    ("Görevleri bir List<Gorev> içinde tut; ekleme, listeleme, tamamlama ve silme işlemlerini ayrı metotlara böl.", "C# Temelleri → 5. ve 6. konular"),
                    ("Kullanıcı geçersiz bir numara girdiğinde program çökmesin: int.TryParse kullan.", "C# Temelleri → 9. konu"),
                    ("Görevleri System.Text.Json ile gorevler.json dosyasına kaydet, program açılırken geri yükle.", "JsonSerializer.Serialize ve JsonSerializer.Deserialize<List<Gorev>>"),
                    ("LINQ ile 'sadece tamamlanmamışları göster' ve 'en eski 3 görev' seçeneklerini ekle.", "C# Temelleri → 10. konu"),
                ]),
        ],

        ["Backend"] =
        [
            P(1, "Kitaplık Web API'si", "Orta",
                "Kitapları yönettiğin, tam CRUD işlemleri yapan bir ASP.NET Core Web API. Bu projede gördüğün yapının aynısını sıfırdan kendin kuracaksın.",
                [
                    ("dotnet new webapi -n Kitaplik.Api --use-controllers ile proje oluştur, şablondaki WeatherForecast dosyalarını sil.", null),
                    ("Kitap entity'si yaz: Id, Ad, Yazar, SayfaSayisi, YayinYili.", null),
                    ("Önce verileri bellekte (List<Kitap>) tutan bir KitaplarController yaz: GET (liste), GET {id}, POST, PUT {id}, DELETE {id}.", "Backend → 2. konu: REST"),
                    ("Doğru durum kodlarını dön: oluşturmada 201 Created, bulunamazsa 404, silmede 204.", "CreatedAtAction(nameof(Getir), new { id = kitap.Id }, kitap)"),
                    ("İş mantığını IKitapServisi interface'i ve KitapServisi sınıfına taşı; Program.cs'te AddScoped ile kaydet.", "Backend → 3. konu: Dependency Injection"),
                    ("KitapOlusturDto ekle; [Required] ve [Range] attribute'larıyla doğrulama yap. Eksik veride 400 dönmeli.", "System.ComponentModel.DataAnnotations"),
                    ("EF Core ve SQL Server'a geç: DbContext yaz, migration oluştur, listeyi veritabanından çek.", "SQL → 4. konu: EF Core ve Migration"),
                    ("GET /api/kitaplar?yazar=...&sayfa=1 ile filtreleme ve sayfalama (Skip/Take) ekle.", null),
                ]),
        ],

        ["SQL ve Veritabanı"] =
        [
            P(1, "Kütüphane Veritabanı Tasarımı", "Orta",
                "SSMS'te sıfırdan bir kütüphane veritabanı tasarlayıp gerçek hayattaki soruları SQL ile cevaplayacaksın.",
                [
                    ("SSMS'te CREATE DATABASE KutuphaneDb ile veritabanı oluştur.", null),
                    ("Yazarlar, Kitaplar, Uyeler ve Oduncler tablolarını CREATE TABLE ile yaz. Primary key'leri IDENTITY yap.", "SQL → 1. konu"),
                    ("Foreign key'leri ekle: Kitaplar.YazarId → Yazarlar, Oduncler.KitapId → Kitaplar, Oduncler.UyeId → Uyeler.", null),
                    ("Her tabloya INSERT ile en az 5 örnek kayıt ekle.", null),
                    ("Sorgu: Şu an ödünçte olan (IadeTarihi IS NULL) kitapları üye adıyla birlikte listele.", "INNER JOIN + WHERE"),
                    ("Sorgu: Her yazarın kaç kitabı var? Hiç kitabı olmayan yazarlar da 0 ile görünsün.", "LEFT JOIN + GROUP BY + COUNT"),
                    ("Sorgu: En çok ödünç alınan 3 kitap.", "GROUP BY + ORDER BY + TOP 3"),
                    ("Oduncler tablosunda UyeId sütununa index ekle; bir sorgunun execution plan'ını index'ten önce ve sonra karşılaştır.", "SQL → 5. konu"),
                    ("Kitap ödünç verme işlemini (Oduncler'e INSERT + Kitaplar.StokAdedi azaltma) BEGIN TRAN / COMMIT ile tek transaction'da yap.", "SQL → 6. konu"),
                ]),
            P(2, "Yol Haritası Veritabanını Sorgula", "Kolay",
                "Bu uygulamanın kendi veritabanında, uygulamadaki Veritabanı sekmesini kullanarak pratik yap.",
                [
                    ("Veritabanı sekmesinde tabloları ve sütunlarını incele; hangi sütunların foreign key olduğunu bul.", null),
                    ("Tüm alanları ve her alandaki konu sayısını listeleyen sorguyu yaz.", "SELECT a.Ad, COUNT(*) FROM Alanlar a JOIN Konular k ON k.AlanId = a.Id GROUP BY a.Ad"),
                    ("Adında 'Key' geçen terimleri ve ait oldukları konunun başlığını getir.", "JOIN + LIKE '%Key%'"),
                    ("Hiç kaynağı olmayan konuları bul.", "LEFT JOIN Kaynaklar ... WHERE kaynak.Id IS NULL"),
                    ("Tamamladığın konuları tamamlanma tarihine göre listele.", null),
                ]),
        ],

        ["Sunucu ve Windows"] =
        [
            P(1, "Yol Haritası API'sini IIS'te Yayınla", "Orta",
                "Bu uygulamayı kendi bilgisayarındaki IIS'e gerçek bir sunucu gibi kuracaksın.",
                [
                    ("'Windows özelliklerini aç veya kapat' ekranından IIS'i etkinleştir; http://localhost ile varsayılan sayfayı gör.", null),
                    ("ASP.NET Core Hosting Bundle'ı kur ve IIS'i yeniden başlat (iisreset).", "Sunucu → 1. konu"),
                    ("dotnet publish -c Release -o C:\\inetpub\\yolharitasi ile uygulamayı yayınla.", null),
                    ("IIS Manager'da YolHaritasi adında bir site ve 'No Managed Code' ayarlı bir Application Pool oluştur.", "Sunucu → 2. konu"),
                    ("Siteyi aç; veritabanı hatası alacaksın. Event Viewer'da hatayı bul ve sebebini (App Pool kimliğinin SQL yetkisi) anla.", "Sunucu → 6. konu"),
                    ("SQL Server'da IIS AppPool\\YolHaritasi kullanıcısına veritabanı yetkisi ver ve siteyi tekrar dene.", null),
                    ("hosts dosyasına yolharitasi.local ekle ve IIS'te bu adla bir binding oluştur.", "Sunucu → 3. konu"),
                ]),
        ],

        ["Yapay Zeka"] =
        [
            P(1, "AI Terim Açıklayıcı", "Orta",
                "Uygulamaya, bir terimi yapay zekaya daha basit anlattıran bir endpoint ekleyeceksin.",
                [
                    ("Bir yapay zeka sağlayıcısından API anahtarı al. Anahtarı dotnet user-secrets ile sakla, asla koda yazma.", "Yapay Zeka → 2. konu"),
                    ("Microsoft.Extensions.AI paketini ekle ve IChatClient'ı Program.cs'te DI'a kaydet.", null),
                    ("POST /api/terimler/{id}/acikla endpoint'i yaz: terimi veritabanından al, modele 'bunu yeni başlayan birine örnekle anlat' de.", "Yapay Zeka → 3. konu: System Prompt"),
                    ("Cevabı JSON formatında (aciklama, ornek, benzerTerimler) iste ve bir DTO'ya çevir.", "Structured Output"),
                    ("Aynı terim için gelen cevabı veritabanında sakla; tekrar sorulursa modele gitmeden oradan dön.", "Yapay Zeka → 6. konu: Caching"),
                ]),
        ],
    };

    private static Proje P(int sira, string baslik, string zorluk, string aciklama, (string Aciklama, string? Ipucu)[] adimlar) => new()
    {
        Sira = sira,
        Baslik = baslik,
        Zorluk = zorluk,
        Aciklama = aciklama,
        Adimlar = adimlar.Select((a, i) => new ProjeAdimi { Sira = i + 1, Aciklama = a.Aciklama, Ipucu = a.Ipucu }).ToList(),
    };
}
