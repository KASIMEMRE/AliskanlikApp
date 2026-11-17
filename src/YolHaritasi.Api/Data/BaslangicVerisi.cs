using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Entities;

namespace YolHaritasi.Api.Data;

/// <summary>
/// SEED DATA (tohum / başlangıç verisi): Veritabanı ilk oluşturulduğunda içine konan hazır veri.
/// Uygulama açılırken çağrılır; Alanlar tablosu BOŞSA yol haritasının tamamını ekler,
/// doluysa hiçbir şey yapmaz. Bu sayede uygulamayı her açtığında veri tekrar tekrar eklenmez
/// (bu özelliğe IDEMPOTENT denir: kaç kez çalışırsa çalışsın sonuç aynı).
/// </summary>
public static class BaslangicVerisi
{
    public static async Task EkleAsync(UygulamaDbContext db)
    {
        // AnyAsync → SQL: SELECT CASE WHEN EXISTS (SELECT 1 FROM Alanlar) ...
        if (await db.Alanlar.AnyAsync())
            return;

        db.Alanlar.AddRange(Backend(), Sql(), Sunucu(), YapayZeka());

        // Bu satıra kadar veritabanına HİÇBİR ŞEY gitmedi; nesneler sadece bellekte "eklenecek" diye işaretlendi.
        // SaveChangesAsync tüm INSERT'leri tek bir TRANSACTION içinde gönderir:
        // ya hepsi kaydedilir ya hiçbiri.
        await db.SaveChangesAsync();
    }

    // ---------------------------------------------------------------------
    //  Yardımcı metot: Konu nesnesini kısa yoldan oluşturmak için.
    // ---------------------------------------------------------------------
    private static Konu K(int sira, string baslik, string ozet, int dakika,
        (string Ad, string Tanim, string? Ornek)[] terimler, string[] gorevler) => new()
    {
        Sira = sira,
        Baslik = baslik,
        Ozet = ozet,
        TahminiDakika = dakika,
        Terimler = terimler.Select(t => new Terim { Ad = t.Ad, Tanim = t.Tanim, Ornek = t.Ornek }).ToList(),
        Gorevler = gorevler.Select((g, i) => new Gorev { Aciklama = g, Sira = i + 1 }).ToList(),
    };

    // =====================================================================
    //  1) BACKEND
    // =====================================================================
    private static Alan Backend() => new()
    {
        Sira = 1,
        Ad = "Backend",
        Aciklama = "C# ve ASP.NET Core ile sunucu tarafı uygulama geliştirme.",
        Konular =
        [
            K(1, "İstemci, Sunucu ve HTTP", "Web'de iki programın nasıl konuştuğunu öğren: istek, cevap ve durum kodları.", 60,
            [
                ("Client (İstemci)", "İsteği gönderen taraf: tarayıcı, mobil uygulama, curl.", "Tarayıcın localhost:5019/api/alanlar adresine istek atar."),
                ("Server (Sunucu)", "İsteği karşılayıp cevap üreten program veya bilgisayar.", "dotnet run ile çalışan YolHaritasi.Api"),
                ("HTTP", "İstemci ile sunucunun konuştuğu metin tabanlı protokol.", null),
                ("Request / Response", "Her HTTP konuşması bir istek (metot, adres, başlıklar, gövde) ve bir cevaptan (durum kodu, başlıklar, gövde) oluşur.", null),
                ("Status Code", "Cevabın sonucunu anlatan sayı. 2xx başarılı, 4xx istemci hatası, 5xx sunucu hatası.", "200 OK, 201 Created, 400 Bad Request, 404 Not Found, 500 Internal Server Error"),
                ("Port", "Aynı bilgisayardaki programları ayıran kapı numarası.", "localhost:5019 → 5019 portu"),
            ],
            [
                "Uygulamayı çalıştır, tarayıcıda F12 → Network sekmesinde bir isteğin Status, Headers ve Response kısımlarını incele.",
                "Olmayan bir adrese istek atıp 404 cevabını gör.",
            ]),

            K(2, "REST API ve Endpoint", "Kaynak (resource) mantığıyla API tasarımı ve HTTP metotları.", 90,
            [
                ("API", "Programların birbiriyle konuşması için sunulan arayüz; hangi adrese ne gönderilirse ne alınacağını tanımlar.", null),
                ("REST", "Veriyi 'kaynak' olarak düşünüp URL'lerle adresleyen, HTTP metotlarıyla işlem yapan API tasarım stili.", "GET /api/konular/3 → 3 numaralı konuyu getir"),
                ("Endpoint", "Bir route + HTTP metodu eşleşmesi. 'Bu adrese bu metotla gelinirse şu kod çalışır.'", "POST /api/konular/3/tamamla"),
                ("HTTP Metotları", "GET = oku, POST = oluştur/işlem yap, PUT = tamamen güncelle, PATCH = kısmen güncelle, DELETE = sil.", null),
                ("JSON", "Verinin metin olarak taşınma biçimi.", "{ \"id\": 1, \"ad\": \"Backend\" }"),
                ("Serialization", "C# nesnesini JSON'a çevirme. Tersi deserialization.", null),
                ("Query String", "URL'de ? işaretinden sonra gelen filtre parametreleri.", "/api/terimler?ara=key"),
            ],
            [
                "Scalar ekranında (/scalar) tüm endpoint'leri incele ve birkaçını dene.",
                "Controllers klasöründeki her [HttpGet]/[HttpPost] satırının hangi URL'ye karşılık geldiğini çıkar.",
            ]),

            K(3, "Dependency Injection", "Nesneleri kendimiz oluşturmak yerine framework'ün bize vermesi.", 90,
            [
                ("Dependency Injection (DI)", "Bir sınıfın ihtiyaç duyduğu nesneleri (bağımlılıkları) kendisi new'lemek yerine dışarıdan, genelde constructor üzerinden alması.", "IlerlemeServisi, UygulamaDbContext'i constructor'dan alır."),
                ("Service Lifetime", "DI'ın bir nesneyi ne kadar yaşatacağı. Singleton: uygulama boyunca tek. Scoped: her HTTP isteği için bir tane. Transient: her istendiğinde yeni.", "DbContext Scoped'dır."),
                ("Interface", "Bir sınıfın 'ne yapabildiğini' tanımlayan sözleşme; nasıl yaptığını içermez.", "IIlerlemeServisi"),
            ],
            [
                "Program.cs'te builder.Services.Add... satırlarını bul; her birinin ne kaydettiğini açıkla.",
            ]),

            K(4, "Katmanlı Mimari ve DTO", "Controller, servis ve veri katmanlarını ayırmak; dışarıya entity değil DTO göndermek.", 90,
            [
                ("Controller", "HTTP isteğini karşılayan, cevabı dönen ince katman. İş kuralı içermemeli.", null),
                ("Service Layer", "İş kurallarının (business logic) yaşadığı katman.", "IlerlemeServisi: 'bugün ne çalışmalıyım' kararını verir."),
                ("DTO", "Data Transfer Object: API'nin dışarıya gönderdiği/aldığı veri şekli. Entity'yi doğrudan açmamak için kullanılır.", "KonuDetayDto"),
                ("Separation of Concerns", "Her parçanın tek bir sorumluluğu olması.", null),
            ],
            [
                "Bir entity ile ona karşılık gelen DTO'yu yan yana açıp farklarını incele.",
            ]),

            K(5, "Kimlik Doğrulama ve Yetkilendirme", "Kullanıcı girişi, şifre güvenliği ve JWT.", 120,
            [
                ("Authentication", "Kimlik doğrulama: 'Sen kimsin?'", "Kullanıcı adı + şifre ile giriş"),
                ("Authorization", "Yetkilendirme: 'Bunu yapmaya iznin var mı?'", "Sadece admin konu ekleyebilir"),
                ("Hash", "Geri döndürülemez tek yönlü dönüşüm. Şifreler veritabanında asla düz metin değil, hash olarak saklanır.", null),
                ("JWT", "JSON Web Token: Sunucunun imzaladığı, kullanıcının kim olduğunu taşıyan kimlik kartı.", "Authorization: Bearer eyJhbGciOi..."),
            ],
            [
                "jwt.io sitesinde örnek bir token'ı çözüp içindeki bilgileri incele.",
            ]),

            K(6, "Middleware, Loglama ve Hata Yönetimi", "İsteğin geçtiği boru hattı ve uygulamayı izlemek.", 90,
            [
                ("Middleware", "Her isteğin sırayla geçtiği ara katmanlar. Loglama, hata yakalama, kimlik kontrolü gibi.", "app.UseHttpsRedirection()"),
                ("Pipeline", "Middleware'lerin sıralı zinciri. Sıra önemlidir.", null),
                ("Logging", "Uygulamanın ne yaptığını kayıt altına almak. Seviyeler: Trace, Debug, Information, Warning, Error, Critical.", null),
            ],
            [
                "dotnet run çıktısındaki log satırlarını oku; appsettings.json'daki LogLevel'i değiştirip farkı gör.",
            ]),
        ],
    };

    // =====================================================================
    //  2) SQL VE VERİTABANI
    // =====================================================================
    private static Alan Sql() => new()
    {
        Sira = 2,
        Ad = "SQL ve Veritabanı",
        Aciklama = "SQL Server, ilişkisel veri modeli ve Entity Framework Core.",
        Konular =
        [
            K(1, "İlişkisel Veritabanı Temelleri", "Tablo, satır, sütun ve anahtar kavramları.", 60,
            [
                ("Tablo", "Aynı türden kayıtların tutulduğu yapı. Excel sayfasına benzer.", "Alanlar, Konular, Terimler, Gorevler"),
                ("Satır / Sütun", "Satır (row) bir kayıttır, sütun (column) kaydın bir özelliğidir.", null),
                ("Primary Key", "Her satırı benzersiz tanımlayan sütun.", "Konular.Id"),
                ("Foreign Key", "Başka bir tablonun primary key'ine işaret eden sütun; tablolar arası ilişkiyi kurar.", "Konular.AlanId → Alanlar.Id"),
                ("NULL", "'Değer yok' demek. 0 veya boş metin ile aynı şey değildir.", "Konular.TamamlanmaTarihi"),
                ("Veri Tipi", "Sütunun ne tür veri tuttuğu.", "int, nvarchar(200), bit, datetime2"),
            ],
            [
                "SSMS ile YolHaritasiDb veritabanına bağlan ve tabloları incele.",
                "Konular tablosunun Keys ve Indexes klasörlerine bak.",
            ]),

            K(2, "Temel SQL Komutları", "Veriyi okumak, filtrelemek, eklemek, güncellemek, silmek.", 90,
            [
                ("SELECT", "Veri okuma.", "SELECT Baslik, Sira FROM Konular"),
                ("WHERE", "Filtreleme.", "SELECT * FROM Konular WHERE TamamlanmaTarihi IS NULL"),
                ("ORDER BY", "Sıralama.", "SELECT * FROM Konular ORDER BY Sira DESC"),
                ("INSERT / UPDATE / DELETE", "Ekleme / güncelleme / silme. UPDATE ve DELETE'de WHERE unutmak tüm tabloyu etkiler!", "UPDATE Gorevler SET Tamamlandi = 1 WHERE Id = 5"),
                ("CRUD", "Create, Read, Update, Delete: veri üzerindeki dört temel işlem.", null),
            ],
            [
                "SSMS'te tamamlanmamış konuları listeleyen bir sorgu yaz.",
                "Uygulamadan bir konuyu tamamla, sonra SSMS'te TamamlanmaTarihi sütununun dolduğunu gör.",
            ]),

            K(3, "JOIN ve İlişki Türleri", "Birden fazla tabloyu birleştirerek sorgulamak.", 90,
            [
                ("JOIN", "İki tabloyu ortak bir sütun üzerinden birleştirme.", "SELECT a.Ad, k.Baslik FROM Konular k JOIN Alanlar a ON a.Id = k.AlanId"),
                ("INNER vs LEFT JOIN", "INNER sadece eşleşenleri, LEFT soldaki tablonun tamamını (eşleşmeyenlerde NULL) getirir.", null),
                ("One-to-Many", "Bire çok ilişki: Bir alanın çok konusu var.", "Alan → Konular"),
                ("Many-to-Many", "Çoka çok ilişki; araya bir bağlantı tablosu gerekir.", "Kullanıcılar ↔ Konular (ileride: KullaniciIlerleme tablosu)"),
                ("GROUP BY / COUNT", "Gruplayıp saymak.", "SELECT AlanId, COUNT(*) FROM Konular GROUP BY AlanId"),
            ],
            [
                "Her alan için toplam konu sayısını ve tamamlanan konu sayısını veren bir SQL yaz.",
            ]),

            K(4, "Entity Framework Core ve Migration", "C# kodundan veritabanı yönetmek.", 120,
            [
                ("ORM", "Object-Relational Mapping: Nesneler ile tablolar arasında otomatik eşleme.", "EF Core, Dapper (mikro ORM)"),
                ("DbContext", "EF Core'da veritabanı bağlantısını, tabloları ve değişiklik takibini yöneten sınıf.", "UygulamaDbContext"),
                ("Migration", "Veritabanı şemasındaki değişikliklerin sürümlenmiş C# dosyaları. Kod gibi Git'te saklanır.", "dotnet ef migrations add IlkKurulum"),
                ("LINQ", "C# içinde sorgu yazma dili; EF Core bunu SQL'e çevirir.", "db.Konular.Where(k => k.AlanId == 1)"),
                ("Connection String", "Uygulamanın veritabanına nasıl bağlanacağını anlatan metin.", "Server=(localdb)\\MSSQLLocalDB;Database=YolHaritasiDb;..."),
            ],
            [
                "Migrations klasöründeki dosyayı aç ve CreateTable çağrılarını entity'lerle karşılaştır.",
                "Loglarda EF Core'un ürettiği SQL sorgularını bul.",
            ]),

            K(5, "Index ve Performans", "Sorguları hızlandırmak ve yavaşlığın nedenini bulmak.", 90,
            [
                ("Index", "Kitabın arkasındaki dizin gibi; aramayı hızlandırır, yazmayı biraz yavaşlatır.", "IX_Terimler_Ad"),
                ("Execution Plan", "SQL Server'ın bir sorguyu nasıl çalıştırdığının planı. Yavaş sorguları buradan teşhis ederiz.", "SSMS → Include Actual Execution Plan"),
                ("N+1 Problemi", "Bir liste için 1 sorgu, sonra her eleman için ayrı birer sorgu atılması. Include veya projeksiyonla çözülür.", null),
            ],
            [
                "Terimler tablosunda Ad ile arama yapan bir sorgunun execution plan'ını incele.",
            ]),

            K(6, "Transaction ve Tutarlılık", "Birden fazla işlemi tek bir bütün olarak yapmak.", 90,
            [
                ("Transaction", "Ya hepsi ya hiçbiri prensibiyle çalışan işlem grubu.", "BEGIN TRAN ... COMMIT / ROLLBACK"),
                ("ACID", "Atomicity, Consistency, Isolation, Durability: güvenilir transaction'ın dört özelliği.", null),
                ("Deadlock", "İki işlemin birbirinin kilidini bekleyip sonsuza kadar takılması.", null),
            ],
            [
                "SSMS'te BEGIN TRAN ile bir UPDATE yap, ROLLBACK ile geri al.",
            ]),
        ],
    };

    // =====================================================================
    //  3) SUNUCU VE WINDOWS
    // =====================================================================
    private static Alan Sunucu() => new()
    {
        Sira = 3,
        Ad = "Sunucu ve Windows",
        Aciklama = "IIS, Windows Service, Registry ve sunucu yönetimi.",
        Konular =
        [
            K(1, "IIS Temelleri", "Microsoft'un web sunucusu IIS'e .NET uygulaması yayınlamak.", 90,
            [
                ("IIS", "Internet Information Services: Windows'un web sunucusu.", null),
                ("Hosting Bundle", "IIS'in ASP.NET Core uygulamalarını çalıştırabilmesi için kurulan paket (runtime + ASP.NET Core Module).", null),
                ("Publish", "Uygulamayı sunucuya konacak hale getirmek.", "dotnet publish -c Release"),
                ("Reverse Proxy", "İsteği önce karşılayıp arkadaki uygulamaya ileten sunucu.", "IIS → Kestrel (out-of-process modda)"),
                ("In-process / Out-of-process", "ASP.NET Core'un IIS işleminin içinde mi (w3wp.exe) yoksa ayrı bir işlemde mi çalıştığı.", null),
            ],
            [
                "Windows Özellikleri'nden IIS'i aç, http://localhost ile varsayılan sayfayı gör.",
                "YolHaritasi.Api'yi publish edip IIS'te yeni site olarak yayınla.",
            ]),

            K(2, "Application Pool ve Kimlikler", "Sitelerin hangi işlemde ve hangi hesapla çalıştığı.", 60,
            [
                ("Application Pool", "Bir veya daha fazla sitenin çalıştığı yalıtılmış işlem (w3wp.exe).", null),
                ("App Pool Identity", "Uygulamanın Windows'ta hangi kullanıcı hesabıyla çalıştığı. Dosya/veritabanı izinleri buna göre verilir.", "IIS AppPool\\YolHaritasi"),
                ("Recycle", "App Pool'un belirli aralıklarla yeniden başlatılması.", null),
            ],
            [
                "App Pool identity'sine SQL Server'da yetki vermeden siteyi çalıştırıp hatayı gör, sonra yetkiyi verip düzelt.",
            ]),

            K(3, "HTTPS, Sertifika ve Binding", "Güvenli bağlantı ve sitenin hangi adreste yayınlanacağı.", 60,
            [
                ("TLS / HTTPS", "Trafiği şifreleyen protokol; HTTPS = HTTP + TLS.", null),
                ("Sertifika", "Sunucunun kimliğini kanıtlayan, güvenilir bir otorite tarafından imzalanmış dosya.", null),
                ("Binding", "IIS'te sitenin protokol + IP + port + host adı eşleşmesi.", "https *:443 yolharitasi.local"),
                ("hosts dosyası", "Bilgisayarda alan adı → IP eşlemesi yapan dosya.", "C:\\Windows\\System32\\drivers\\etc\\hosts"),
            ],
            [
                "hosts dosyasına yolharitasi.local ekle ve IIS'te bu adla binding oluştur.",
            ]),

            K(4, "Worker Service ve Windows Service", "Arka planda sürekli çalışan programlar.", 90,
            [
                ("Windows Service", "Kullanıcı oturum açmasa bile arka planda çalışan Windows programı.", "services.msc"),
                ("Worker Service", ".NET'in arka plan işi şablonu; BackgroundService sınıfından türetilir.", null),
                ("sc.exe", "Windows servislerini komut satırından kurma/başlatma/silme aracı.", "sc.exe create YolHaritasiHatirlatici binPath= \"C:\\...\\Worker.exe\""),
            ],
            [
                "Her sabah 'bugünün konusunu' hesaplayıp log'a yazan bir Worker Service'i Windows Service olarak kur.",
            ]),

            K(5, "Registry", "Windows'un merkezi ayar veritabanı.", 60,
            [
                ("Registry", "Windows ve uygulamaların ayarlarının tutulduğu hiyerarşik veritabanı.", "regedit"),
                ("Hive", "Registry'nin ana kökleri.", "HKEY_LOCAL_MACHINE (HKLM), HKEY_CURRENT_USER (HKCU)"),
                ("Key / Value", "Key klasör gibidir, Value içindeki ad-veri çiftidir.", "HKLM\\SOFTWARE\\YolHaritasi → HatirlatmaSaati = 09:00"),
            ],
            [
                "regedit ile HKCU\\Software altına bir test anahtarı oluştur ve C# ile oku.",
            ]),

            K(6, "Event Viewer, PowerShell ve İzleme", "Sunucuda olanı görmek ve otomatikleştirmek.", 60,
            [
                ("Event Viewer", "Windows olay günlüklerini gösteren araç. IIS ve servis hataları buraya düşer.", "eventvwr.msc"),
                ("PowerShell", "Windows'un betik (script) ve otomasyon kabuğu.", "Get-Service, Restart-WebAppPool"),
                ("Task Scheduler", "Belirli zamanlarda program çalıştıran Windows aracı.", null),
            ],
            [
                "Event Viewer → Windows Logs → Application'da IIS/.NET kaynaklı bir kaydı bul.",
            ]),
        ],
    };

    // =====================================================================
    //  4) YAPAY ZEKA
    // =====================================================================
    private static Alan YapayZeka() => new()
    {
        Sira = 4,
        Ad = "Yapay Zeka",
        Aciklama = "LLM'leri .NET backend'ine entegre etmek.",
        Konular =
        [
            K(1, "LLM Temelleri", "Büyük dil modelleri nasıl çalışır, neyi iyi yapar, neyi yapamaz.", 60,
            [
                ("LLM", "Large Language Model: Büyük metin verisiyle eğitilmiş, metin üreten model.", "Claude, GPT"),
                ("Token", "Modelin işlediği metin parçası (kelime veya kelime parçası). Ücretlendirme token'la yapılır.", null),
                ("Prompt", "Modele gönderilen talimat ve içerik.", null),
                ("Context Window", "Modelin tek seferde dikkate alabildiği maksimum token miktarı.", null),
                ("Hallucination", "Modelin kendinden emin şekilde yanlış bilgi üretmesi.", null),
            ],
            [
                "Bir LLM'e aynı soruyu farklı şekillerde sorup cevapların nasıl değiştiğini gözlemle.",
            ]),

            K(2, ".NET'ten LLM Çağırmak", "Backend'den bir yapay zeka API'sine istek atmak.", 90,
            [
                ("API Key", "Bir servise erişim için gizli anahtar. Asla koda ve Git'e yazılmaz.", "User Secrets veya ortam değişkeni"),
                ("Microsoft.Extensions.AI", ".NET'in farklı AI sağlayıcılarını tek arayüzle (IChatClient) kullanmayı sağlayan kütüphanesi.", null),
                ("Streaming", "Cevabın tamamını beklemeden parça parça almak.", null),
            ],
            [
                "Uygulamaya 'bu terimi bana daha basit anlat' endpoint'i ekle (ileride birlikte yapacağız).",
            ]),

            K(3, "Prompt Mühendisliği ve Yapılandırılmış Çıktı", "Modelden tutarlı ve işlenebilir cevap almak.", 90,
            [
                ("System Prompt", "Modelin rolünü ve kurallarını belirleyen talimat.", "'Sen sabırlı bir backend mentorusun.'"),
                ("Structured Output", "Modelin cevabını belirli bir JSON şemasına uygun vermesi.", null),
                ("Few-shot", "Prompt'a birkaç örnek koyarak modele istenen formatı göstermek.", null),
            ],
            [
                "AI'dan bir konu için 3 soruluk JSON formatında quiz üretmesini iste.",
            ]),

            K(4, "Tool Calling ve Agent", "Modelin bizim fonksiyonlarımızı çağırması.", 120,
            [
                ("Tool Calling", "Modelin, tanımladığımız bir fonksiyonu (ör. 'konuyu tamamla') çağırmaya karar vermesi.", null),
                ("Agent", "Hedefe ulaşmak için tool'ları döngü içinde kullanan yapay zeka sistemi.", null),
            ],
            [
                "AI mentora 'bugünkü görevlerimi listele' tool'u ver.",
            ]),

            K(5, "Embedding ve RAG", "Kendi verimizi yapay zekaya bağlamak.", 120,
            [
                ("Embedding", "Metnin anlamını temsil eden sayı dizisi (vektör). Benzer anlamlı metinlerin vektörleri birbirine yakındır.", null),
                ("Vektör Arama", "Anlamca en yakın kayıtları bulmak.", "SQL Server 2025 VECTOR tipi"),
                ("RAG", "Retrieval-Augmented Generation: Önce ilgili veriyi bul, sonra prompt'a ekleyip modele cevaplat.", "Terim sözlüğümüzden ilgili terimleri bulup AI'a vermek"),
            ],
            [
                "Terimler için embedding üretip 'anlamca benzer terimler' özelliği ekle.",
            ]),

            K(6, "AI Güvenliği ve Maliyet", "Üretimde AI kullanırken dikkat edilecekler.", 60,
            [
                ("Prompt Injection", "Kullanıcı girdisinin, modelin talimatlarını ele geçirmeye çalışması.", null),
                ("Rate Limit", "Belirli sürede yapılabilecek istek sınırı.", "429 Too Many Requests"),
                ("Caching", "Aynı soruların cevaplarını saklayıp maliyeti düşürmek.", null),
            ],
            [
                "AI endpoint'ine kullanıcı başına istek sınırı ekle.",
            ]),
        ],
    };
}
