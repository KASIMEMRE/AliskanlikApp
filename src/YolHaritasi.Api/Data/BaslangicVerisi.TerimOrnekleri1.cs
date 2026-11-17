namespace YolHaritasi.Api.Data;

// ============================================================================
//  TERİM KULLANIM ÖRNEKLERİ (1/2): C# Temelleri ve Backend
//  Anahtar: Terim adı. Değer: (Nerede kullanılır, Kod dili, Kod örneği)
//  Aynı adlı terim birden fazla konuda geçiyorsa (ör. Interface) aynı örnek kullanılır.
// ============================================================================
public static partial class BaslangicVerisi
{
    private static (string Kullanim, string Dil, string Kod) T(string kullanim, string dil, string kod) => (kullanim, dil, kod);

    private static readonly Dictionary<string, (string Kullanim, string Dil, string Kod)> TerimOrnekleri1 = new()
    {
        // ======================= C# TEMELLERİ =======================
        [".NET"] = T("Bir C# projesinin hangi .NET sürümünü kullandığı .csproj dosyasında yazar. Sunucuya kurulacak runtime sürümünü buna göre seçersin.", "text", """
            <!-- YolHaritasi.Api.csproj -->
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>

            # Bilgisayarda kurulu runtime'ları görmek:
            dotnet --list-runtimes
            """),

        ["SDK"] = T("Geliştirici bilgisayarına SDK, sunucuya sadece runtime kurulur. 'Bu projeyi derleyemiyorum' hatalarında ilk kontrol edilen şey SDK sürümüdür.", "powershell", """
            dotnet --version          # aktif SDK sürümü: 10.0.115
            dotnet --list-sdks        # kurulu tüm SDK'lar
            dotnet new webapi -n Api  # SDK ile yeni proje oluştur
            dotnet build              # SDK içindeki derleyiciyle derle
            """),

        ["Compiler"] = T("Kodunda yazım hatası varsa uygulama hiç çalışmaz; derleyici hatayı dosya ve satır numarasıyla verir. Derleme hatalarını okumak günlük işin parçasıdır.", "text", """
            > dotnet build
            Program.cs(12,17): error CS0103: The name 'konular' does not exist in the current context
            Program.cs(15,9):  error CS0029: Cannot implicitly convert type 'string' to 'int'

            # CS0103 → Böyle bir değişken yok (yazım hatası? tanımlanmamış?)
            # CS0029 → Tip uyuşmazlığı (string bir değeri int değişkene atamaya çalışmışsın)
            """),

        ["Console Application"] = T("Veritabanı aktarım araçları, zamanlanmış toplu işler (batch job), küçük yardımcı araçlar genelde konsol uygulaması olarak yazılır.", "csharp", """
            // Program.cs — her gün çalışıp eski logları silen bir araç
            var klasor = args.Length > 0 ? args[0] : @"C:\Loglar";
            var sinir = DateTime.Now.AddDays(-30);

            foreach (var dosya in Directory.GetFiles(klasor, "*.log"))
            {
                if (File.GetLastWriteTime(dosya) < sinir)
                {
                    File.Delete(dosya);
                    Console.WriteLine($"Silindi: {dosya}");
                }
            }
            // Çalıştırma: dotnet run -- D:\UygulamaLoglari
            """),

        ["Namespace"] = T("Büyük projelerde aynı isimli sınıflar olabilir (ör. iki farklı 'Kullanici' sınıfı). Namespace bunları ayırır; dosyanın en üstündeki 'using' satırları hangi namespace'lerin kullanıldığını söyler.", "csharp", """
            // Entities/Konu.cs
            namespace YolHaritasi.Api.Entities;
            public class Konu { ... }

            // Controllers/KonularController.cs
            using YolHaritasi.Api.Dtos;        // KonuDetayDto burada
            using YolHaritasi.Api.Services;    // IIlerlemeServisi burada

            namespace YolHaritasi.Api.Controllers;
            """),

        ["Variable"] = T("Her metodun içinde, bir isteğin parametrelerini, veritabanından gelen sonucu, hesaplanan değerleri değişkenlerde tutarsın.", "csharp", """
            // IlerlemeServisi.cs — gerçek kod
            var toplam = await db.Konular.CountAsync();
            var tamamlanan = await db.Konular.CountAsync(k => k.TamamlanmaTarihi != null);
            var yuzde = toplam == 0 ? 0 : tamamlanan * 100 / toplam;
            """),

        ["Data Type"] = T("C#'ta her değişkenin, SQL'de her sütunun bir tipi var ve ikisi eşleşmeli. EF Core bu eşlemeyi otomatik yapar.", "text", """
            C# tipi           →  SQL Server tipi       Örnek (bu projeden)
            ---------------------------------------------------------------
            int               →  int                   Konu.Id, Konu.Sira
            string            →  nvarchar(n) / (max)   Konu.Baslik → nvarchar(200)
            string?           →  nvarchar NULL         Terim.Ornek
            bool              →  bit                   Gorev.Tamamlandi
            DateTime?         →  datetime2 NULL        Konu.TamamlanmaTarihi
            decimal           →  decimal(18,2)         (para: Urun.Fiyat)
            Guid              →  uniqueidentifier      (dağıtık sistemlerde Id)
            """),

        ["var"] = T("Modern C# kodunda tipi sağ taraftan belli olan değişkenlerin çoğu var ile yazılır. Özellikle uzun generic tiplerde okunabilirliği artırır.", "csharp", """
            // Uzun hali:
            Dictionary<string, List<KonuOzetDto>> alanKonulari = new Dictionary<string, List<KonuOzetDto>>();

            // var ile:
            var alanKonulari = new Dictionary<string, List<KonuOzetDto>>();

            // Tipi belli olmayan yerde var kullanma (okuyan anlamaz):
            var sonuc = Hesapla();          // ❌ sonuç ne tipte?
            decimal toplamTutar = Hesapla(); // ✅
            """),

        ["Type Casting"] = T("Kullanıcıdan, URL'den, dosyadan, ayar dosyasından gelen veri hep metindir; sayıya, tarihe çevirmen gerekir.", "csharp", """
            // appsettings.json'dan gelen değer metindir
            string? deger = builder.Configuration["Hatirlatma:SaatFarki"];   // "3"
            int saatFarki = int.Parse(deger ?? "0");

            // Ondalık → tam sayı (kesirli kısım atılır)
            double ortalama = 87.6;
            int yuvarlak = (int)Math.Round(ortalama);   // 88

            // Enum ↔ int
            var zorluk = (Zorluk)2;                     // Zorluk.Zor
            int sayi = (int)Zorluk.Orta;                // 1
            """),

        ["Null"] = T("Veritabanında boş olabilen her sütun, C#'ta nullable tiptir. NullReferenceException, .NET'te en sık görülen hatadır; null kontrolü bu yüzden önemlidir.", "csharp", """
            // KonularController.cs — gerçek kod
            var konu = await ilerleme.KonuDetayAsync(id);   // bulunamazsa null döner
            return konu is null ? NotFound() : konu;

            // Null ile çalışmanın kısa yolları
            string baslik = konu?.Baslik ?? "(başlıksız)";  // ?. null ise dur, ?? null ise varsayılan
            int uzunluk = konu?.Baslik?.Length ?? 0;
            """),

        ["Operator"] = T("İş kurallarının neredeyse hepsi operatörlerle yazılır: indirim hesabı, yetki kontrolü, tarih karşılaştırması...", "csharp", """
            // Sepet toplamı ve kargo kuralı
            decimal toplam = urunler.Sum(u => u.Fiyat * u.Adet);
            bool ucretsizKargo = toplam >= 500 || kullanici.PremiumMu;

            // Yetki: admin VEYA kaydın sahibi
            if (kullanici.Rol == "Admin" || kayit.SahibiId == kullanici.Id) { ... }

            // Kalan gün (mod ile döngüsel hesap)
            int haftaninGunu = (bugun + 3) % 7;
            """),

        ["String Interpolation"] = T("Log mesajları, kullanıcıya dönen hata mesajları, e-posta şablonları hep interpolation ile oluşturulur.", "csharp", """
            // Controller'daki gerçek hata mesajı
            return NotFound(new HataDto($"{id} numaralı konu bulunamadı."));

            // Biçimlendirme ile
            var mesaj = $"Toplam: {toplam:N2} TL, Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}";
            // "Toplam: 1.250,50 TL, Tarih: 17.11.2026 14:30"

            // ⚠️ SQL yazarken ASLA interpolation ile değer yapıştırma (SQL Injection):
            // $"SELECT * FROM Kullanicilar WHERE Ad = '{ad}'"   ❌
            """),

        ["Immutable"] = T("DTO'lar ve ayar nesneleri genelde değiştirilemez yapılır; böylece bir yerde yanlışlıkla değiştirilip başka yeri bozmaz. Döngüde string birleştirmek yerine StringBuilder kullanılmasının nedeni de budur.", "csharp", """
            // record DTO → değiştirilemez
            var dto = new TerimDto(1, "API", "...", null, null, null, null);
            // dto.Ad = "x";   ❌ derleme hatası
            var kopya = dto with { Ad = "REST" };   // değiştirilmiş KOPYA oluşturur

            // Döngüde string birleştirme: her seferinde yeni string oluşur (yavaş)
            var sb = new System.Text.StringBuilder();
            foreach (var k in konular) sb.AppendLine(k.Baslik);
            string rapor = sb.ToString();
            """),

        ["Method Chaining"] = T("LINQ sorguları, ASP.NET Core ayarları ve EF Core yapılandırması hep zincirleme metotlarla yazılır.", "csharp", """
            // UygulamaDbContext.cs — gerçek kod
            e.HasOne(x => x.Alan)
             .WithMany(a => a.Konular)
             .HasForeignKey(x => x.AlanId)
             .OnDelete(DeleteBehavior.Cascade);

            // Metin temizleme
            var temiz = girdi.Trim().ToLowerInvariant().Replace(" ", "-");   // "Yeni Konu " → "yeni-konu"
            """),

        ["Condition"] = T("Doğrulama (validation), yetki kontrolü ve iş kuralları hep if ile yazılır. Backend'de en çok 'erken dönüş' (guard clause) kalıbını görürsün.", "csharp", """
            // Guard clause: Sorunlu durumları en başta ele al, mutlu yolu sona bırak
            public async Task<IActionResult> Tamamla(int id)
            {
                if (id <= 0)
                    return BadRequest("Geçersiz id");

                var konu = await db.Konular.FindAsync(id);
                if (konu is null)
                    return NotFound();

                if (konu.TamamlanmaTarihi is not null)
                    return NoContent();          // zaten tamamlanmış

                konu.TamamlanmaTarihi = DateTime.UtcNow;
                await db.SaveChangesAsync();
                return NoContent();
            }
            """),

        ["Switch Expression"] = T("Durum kodlarını mesaja, enum değerlerini metne, rolleri yetkilere çevirmek gibi eşlemelerde kullanılır.", "csharp", """
            // VeritabaniServisi.cs — gerçek kod
            private static string TipAdi(string tip, short maxUzunluk) => tip switch
            {
                "nvarchar" or "nchar" => maxUzunluk == -1 ? $"{tip}(max)" : $"{tip}({maxUzunluk / 2})",
                "varchar" or "char"   => maxUzunluk == -1 ? $"{tip}(max)" : $"{tip}({maxUzunluk})",
                _ => tip,
            };
            """),

        ["Loop"] = T("Veritabanından gelen kayıtları işlemek, dosya satırlarını okumak, yeniden deneme (retry) mantığı döngülerle yazılır.", "csharp", """
            // BaslangicVerisi.cs — gerçek kod: her konu için eksik ders notunu doldur
            foreach (var konu in konular)
            {
                var anahtar = (konu.Alan.Ad, konu.Sira);
                if (konu.DersNotu is null && DersNotlari.TryGetValue(anahtar, out var not))
                    konu.DersNotu = not;
            }

            // Retry: dış servis 3 kez denensin
            for (int deneme = 1; deneme <= 3; deneme++)
            {
                try { return await servis.GonderAsync(); }
                catch when (deneme < 3) { await Task.Delay(1000 * deneme); }
            }
            """),

        ["break / continue"] = T("Büyük listelerde aradığını bulunca durmak (break) veya geçersiz kayıtları atlamak (continue) için kullanılır.", "csharp", """
            // VeritabaniServisi.cs — gerçek kod: 200 satırdan fazlasını okuma
            while (await okuyucu.ReadAsync())
            {
                if (satirlar.Count == MaksimumSatir) { kesildi = true; break; }
                satirlar.Add(...);
            }

            // CSV içe aktarma: boş satırları atla
            foreach (var satir in File.ReadLines("veri.csv"))
            {
                if (string.IsNullOrWhiteSpace(satir)) continue;
                Isle(satir);
            }
            """),

        ["Array"] = T("Sabit boyutlu veriler için: komut satırı argümanları (args), metin bölme (Split) sonuçları, sabit ayar listeleri.", "csharp", """
            // Komut satırı argümanları bir dizidir
            // dotnet run -- rapor 2026
            string komut = args[0];    // "rapor"
            int yil = int.Parse(args[1]);

            // Split → dizi
            string[] parcalar = "Ad;Soyad;Email".Split(';');

            // BaslangicVerisi.cs — gerçek kod (collection expression)
            Alan[] tumAlanlar = [CSharpTemelleri(), Backend(), Sql(), Sunucu(), YapayZeka()];
            """),

        ["List<T>"] = T("API'den dönen listeler, veritabanı sorgu sonuçları, DTO içindeki alt listeler hep List<T>'dir.", "csharp", """
            // Dtolar.cs — gerçek kod
            public record AlanDetayDto(int Id, string Ad, string Aciklama, List<KonuOzetDto> Konular);

            // Sorgu sonucunu listeye çevirme
            List<Konu> konular = await db.Konular.Where(k => k.AlanId == 1).ToListAsync();

            konular.Add(yeniKonu);
            konular.RemoveAll(k => k.TamamlanmaTarihi != null);
            Console.WriteLine($"{konular.Count} konu kaldı");
            """),

        ["Dictionary<TKey, TValue>"] = T("Hızlı arama gereken her yerde: önbellek (cache), kod → açıklama eşlemesi, gruplanmış veriler.", "csharp", """
            // BaslangicVerisi.DersNotlari.cs — gerçek kod: (alan, sıra) → ders notu
            private static readonly Dictionary<(string Alan, int Sira), string> DersNotlari = new()
            {
                [("C# Temelleri", 1)] = "## C# nedir? ...",
            };

            if (DersNotlari.TryGetValue(("Backend", 2), out var not))
                Console.WriteLine(not);

            // Hata kodu → mesaj
            var mesajlar = new Dictionary<int, string> { [404] = "Bulunamadı", [500] = "Sunucu hatası" };
            """),

        ["Generic"] = T("Tekrar eden kodu tipten bağımsız yazmak için. List<T>, Task<T>, ActionResult<T>, ILogger<T>, DbSet<T> hep generic'tir.", "csharp", """
            // ASP.NET Core'daki generic tipler (bu projeden)
            DbSet<Konu> Konular                       // T = Konu
            Task<List<AlanOzetDto>> Listele()         // T = List<AlanOzetDto>
            ActionResult<KonuDetayDto> Getir(int id)  // T = KonuDetayDto
            ILogger<IlerlemeServisi> logger           // T = log kaynağı

            // Kendi generic metodun
            public static T IlkVeyaVarsayilan<T>(List<T> liste, T varsayilan)
                => liste.Count > 0 ? liste[0] : varsayilan;
            """),

        ["Index"] = T("Dizi ve listelerde elemana sıra numarasıyla erişmek. En sık hata: IndexOutOfRangeException (olmayan indeks).", "csharp", """
            var konular = new List<string> { "HTTP", "REST", "DI" };
            Console.WriteLine(konular[0]);   // HTTP  (ilk eleman)
            Console.WriteLine(konular[^1]);  // DI    (sondan birinci)
            Console.WriteLine(konular[3]);   // ❌ ArgumentOutOfRangeException

            // Güvenli erişim
            var ucuncu = konular.ElementAtOrDefault(3);   // null döner, hata vermez
            """),

        ["Method"] = T("Backend kodunun tamamı metotlardan oluşur: her endpoint bir metot, her iş kuralı bir servis metodu.", "csharp", """
            // IlerlemeServisi.cs — gerçek kod (imzası)
            public async Task<bool> KonuyuTamamlaAsync(int konuId)
            //     ↑      ↑           ↑                  ↑
            //  erişim  dönüş tipi   ad              parametre
            {
                var konu = await db.Konular.FindAsync(konuId);
                if (konu is null) return false;
                ...
                return true;
            }
            """),

        ["Parameter / Argument"] = T("Controller metotlarının parametreleri HTTP isteğinden (URL, query string, body) otomatik doldurulur.", "csharp", """
            // Parametreler: id ve istek
            [HttpPatch("{id:int}")]
            public async Task<IActionResult> DurumDegistir(int id, [FromBody] GorevDurumIstegi istek)

            // PATCH /api/gorevler/7  gövde: {"tamamlandi": true}
            // → argümanlar: id = 7, istek = new GorevDurumIstegi(true)
            """),

        ["Return Type"] = T("Controller metotlarının dönüş tipi API'nin ne döndürdüğünü belirler ve Swagger/OpenAPI dokümanında görünür.", "csharp", """
            // Her zaman 200 + liste
            public async Task<List<AlanOzetDto>> Listele()

            // 200 + veri VEYA 404 gibi başka bir sonuç
            public async Task<ActionResult<KonuDetayDto>> Getir(int id)

            // Sadece durum kodu (204, 404...), gövde yok
            public async Task<IActionResult> Tamamla(int id)
            """),

        ["Overloading"] = T(".NET kütüphanelerinde çok yaygındır; aynı işi farklı parametrelerle yapmanı sağlar. Console.WriteLine'ın 18 farklı overload'u vardır.", "csharp", """
            // Kendi servisinde
            public Task<List<Konu>> GetirAsync()              => db.Konular.ToListAsync();
            public Task<List<Konu>> GetirAsync(int alanId)    => db.Konular.Where(k => k.AlanId == alanId).ToListAsync();
            public Task<List<Konu>> GetirAsync(string arama)  => db.Konular.Where(k => k.Baslik.Contains(arama)).ToListAsync();

            // .NET'teki örnekler
            logger.LogInformation("Mesaj");
            logger.LogInformation("Konu {Id}", 5);
            logger.LogInformation(ex, "Hata oluştu");
            """),

        ["Expression-bodied Member"] = T("Tek satırlık metotlar ve property'ler modern C# kodunda neredeyse hep bu şekilde yazılır.", "csharp", """
            // BugunController.cs — gerçek kod
            [HttpGet]
            public Task<BugunDto> Getir() => ilerleme.BugunAsync();

            // Hesaplanan property
            public bool TamamlandiMi => TamamlanmaTarihi is not null;
            public string TamAd => $"{Ad} {Soyad}";
            """),

        ["OOP"] = T("ASP.NET Core'un kendisi OOP üzerine kurulu: controller'lar ControllerBase'den türer, servisler interface'leri uygular, entity'ler tabloları temsil eden sınıflardır.", "csharp", """
            // Bu projede OOP'nin dört temeli:
            public class Konu { public string Baslik { get; set; } }          // Sınıf / nesne
            public class KonularController : ControllerBase { }              // Kalıtım
            public class IlerlemeServisi : IIlerlemeServisi { }              // Interface / soyutlama
            public decimal Bakiye { get; private set; }                     // Kapsülleme
            """),

        ["Class"] = T("Her entity, controller, servis ve DTO bir sınıftır. Yeni bir özellik eklemek çoğu zaman yeni bir sınıf yazmak demektir.", "csharp", """
            // Entities/Gorev.cs — gerçek kod
            public class Gorev
            {
                public int Id { get; set; }
                public string Aciklama { get; set; } = "";
                public int Sira { get; set; }
                public bool Tamamlandi { get; set; }

                public int KonuId { get; set; }
                public Konu Konu { get; set; } = null!;
            }
            """),

        ["Object / Instance"] = T("Veritabanından okunan her satır bir nesneye, API'ye gelen her JSON bir nesneye dönüşür.", "csharp", """
            // BaslangicVerisi.Projeler.cs — gerçek kod: yeni nesneler oluşturuluyor
            new ProjeAdimi { Sira = 1, Aciklama = "dotnet new console ...", Ipucu = null };

            // Aynı sınıftan iki AYRI nesne
            var k1 = await db.Konular.FindAsync(1);
            var k2 = await db.Konular.FindAsync(2);
            k1.Baslik = "Değişti";   // k2 etkilenmez
            """),

        ["Constructor"] = T("Backend'de constructor'ın en önemli görevi bağımlılıkları (DbContext, logger, servisler) almaktır; Dependency Injection constructor üzerinden çalışır.", "csharp", """
            // Klasik constructor
            public class IlerlemeServisi : IIlerlemeServisi
            {
                private readonly UygulamaDbContext _db;
                public IlerlemeServisi(UygulamaDbContext db) { _db = db; }
            }

            // Primary constructor (bu projede kullanılan kısa yazım, C# 12)
            public class IlerlemeServisi(UygulamaDbContext db, ILogger<IlerlemeServisi> logger) : IIlerlemeServisi
            { ... }
            """),

        ["Property"] = T("Entity'lerdeki her property bir tablo sütunudur; DTO'lardaki her property JSON'da bir alandır.", "csharp", """
            public class Konu
            {
                public int Id { get; set; }                           // otomatik property
                public string Baslik { get; set; } = "";              // varsayılan değerli
                public DateTime? TamamlanmaTarihi { get; set; }       // nullable
                public bool TamamlandiMi => TamamlanmaTarihi != null;  // hesaplanan (sütun olmaz*)
                public decimal Bakiye { get; private set; }           // dışarıdan sadece okunur
            }
            // * EF Core'a [NotMapped] ile veya get-only olduğu için sütun yapmamasını söyleriz
            """),

        ["Access Modifier"] = T("Bir sınıfın dışarıya neyi açıp neyi gizleyeceğine karar vermek. Yardımcı metotlar private, API'nin kullandıkları public olur.", "csharp", """
            // VeritabaniServisi.cs — gerçek kod
            public partial class VeritabaniServisi
            {
                private const int MaksimumSatir = 200;                      // sadece bu sınıf
                public async Task<VeritabaniDurumDto> DurumAsync() { }    // controller çağırıyor
                private static async Task<List<TabloDto>> TablolariOkuAsync(...) { }  // iç yardımcı
            }
            """),

        ["Encapsulation"] = T("Kuralları bozulmaması gereken veriyi (bakiye, stok, durum) sadece metotlar üzerinden değiştirilebilir yapmak.", "csharp", """
            public class Siparis
            {
                public string Durum { get; private set; } = "Yeni";

                public void Onayla()
                {
                    if (Durum != "Yeni")
                        throw new InvalidOperationException("Sadece yeni sipariş onaylanabilir");
                    Durum = "Onaylandı";
                }
            }
            // siparis.Durum = "Kargoda";   ❌ derlenmez, kurala uymak zorundasın
            """),

        ["Inheritance"] = T("Framework sınıflarını genişletmek için: controller'lar ControllerBase'den, DbContext'in DbContext'ten, arka plan işleri BackgroundService'ten türer.", "csharp", """
            // Bu projede
            public class KonularController(...) : ControllerBase { }   // NotFound(), Ok() buradan gelir
            public class UygulamaDbContext(...) : DbContext(options) { }

            // Adım 6'da yazacağımız arka plan servisi
            public class HatirlaticiServis : BackgroundService
            {
                protected override async Task ExecuteAsync(CancellationToken iptal) { ... }
            }
            """),

        ["Interface"] = T("Servisleri soyutlamak, test edilebilir kod yazmak ve Dependency Injection için. Backend projelerinde neredeyse her servisin bir interface'i olur.", "csharp", """
            // Services/IlerlemeServisi.cs — gerçek kod
            public interface IIlerlemeServisi
            {
                Task<BugunDto> BugunAsync();
                Task<bool> KonuyuTamamlaAsync(int konuId);
            }
            public class IlerlemeServisi(...) : IIlerlemeServisi { ... }

            // Program.cs: "IIlerlemeServisi isteyene IlerlemeServisi ver"
            builder.Services.AddScoped<IIlerlemeServisi, IlerlemeServisi>();

            // Controller sadece interface'i bilir
            public class KonularController(IIlerlemeServisi ilerleme) : ControllerBase
            """),

        ["Polymorphism"] = T("Aynı arayüzün farklı uygulamalarını tek kodla kullanmak: farklı ödeme yöntemleri, farklı bildirim kanalları, test için sahte servisler.", "csharp", """
            public interface IOdemeYontemi { Task<bool> OdeAsync(decimal tutar); }
            public class KrediKarti : IOdemeYontemi { ... }
            public class Havale : IOdemeYontemi { ... }

            // Sipariş servisi hangisi olduğunu bilmez
            public async Task SiparisVer(IOdemeYontemi yontem, decimal tutar)
            {
                if (await yontem.OdeAsync(tutar))   // gerçek tipe göre farklı kod çalışır
                    ...
            }
            """),

        ["Abstract Class"] = T("Ortak kodu paylaşan ama kendisi tek başına anlamlı olmayan temel sınıflar için. Örnek: tüm entity'lerde ortak Id ve OlusturmaTarihi.", "csharp", """
            public abstract class TemelEntity
            {
                public int Id { get; set; }
                public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
            }

            public class Konu : TemelEntity { public string Baslik { get; set; } = ""; }
            public class Terim : TemelEntity { public string Ad { get; set; } = ""; }

            // new TemelEntity();   ❌ abstract sınıf new'lenemez
            """),

        ["Enum"] = T("Sabit seçenekli alanlar için: sipariş durumu, kullanıcı rolü, zorluk seviyesi. Veritabanında int olarak saklanır.", "csharp", """
            public enum Zorluk { Kolay = 0, Orta = 1, Zor = 2 }

            public class Proje
            {
                public Zorluk Zorluk { get; set; }   // SQL'de int sütunu
            }

            // Kullanım
            var zorProjeler = db.Projeler.Where(p => p.Zorluk == Zorluk.Zor);
            // EF Core'da metin olarak saklamak için: .HasConversion<string>()
            """),

        ["record"] = T("API'nin aldığı ve döndürdüğü veri şekilleri (DTO), değer nesneleri ve mesajlar için. Bu projedeki tüm DTO'lar record.", "csharp", """
            // Dtos/Dtolar.cs — gerçek kod
            public record KaynakDto(string Baslik, string Url, string Site);
            public record HataDto(string Mesaj);

            // İstek gövdesi için (GorevlerController.cs)
            public record GorevDurumIstegi(bool Tamamlandi);
            // Gelen JSON {"tamamlandi": true} otomatik olarak bu record'a dönüşür
            """),

        ["Exception"] = T("Beklenmeyen durumlarda (veritabanı kapalı, dosya yok, geçersiz veri) .NET exception fırlatır. Logda gördüğün 'stack trace' exception'ın nerede oluştuğunu gösterir.", "text", """
            fail: Microsoft.AspNetCore.Server.Kestrel[13]
            System.InvalidOperationException: Sequence contains no elements
               at System.Linq.ThrowHelper.ThrowNoElementsException()
               at YolHaritasi.Api.Services.IlerlemeServisi.BugunAsync() in IlerlemeServisi.cs:line 42
                                                                           ↑ hatanın olduğu yer
            # Sebep: First() boş listede çağrıldı. Çözüm: FirstOrDefault() + null kontrolü
            """),

        ["try / catch / finally"] = T("Dış sistemlerle (veritabanı, başka API, dosya) konuşulan yerlerde. Yakalanan hata loglanır ve kullanıcıya anlamlı bir mesaj döner.", "csharp", """
            // VeritabaniServisi.cs — gerçek kod
            try
            {
                await using var okuyucu = await komut.ExecuteReaderAsync();
                ...
            }
            catch (SqlException ex)
            {
                throw new SorguHatasi($"SQL Server hatası: {ex.Message}");
            }
            finally
            {
                await islem.RollbackAsync();    // her durumda geri al
                await baglanti.CloseAsync();    // her durumda bağlantıyı kapat
            }
            """),

        ["throw"] = T("İş kuralı ihlallerinde ve geçersiz parametrelerde bilerek hata fırlatılır; üst katman bunu yakalayıp uygun HTTP cevabına çevirir.", "csharp", """
            // VeritabaniServisi.cs — gerçek kod
            if (!sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                throw new SorguHatasi("Sadece SELECT sorgularına izin veriliyor.");

            // Controller'da yakalanıp 400'e çevriliyor
            catch (SorguHatasi ex) { return BadRequest(new HataDto(ex.Message)); }

            // Parametre kontrolü
            ArgumentNullException.ThrowIfNull(konu);
            ArgumentOutOfRangeException.ThrowIfNegative(adet);
            """),

        ["TryParse"] = T("Kullanıcı girdisi, URL parametresi, dosyadan okunan değer gibi 'bozuk olabilecek' verileri çevirirken exception yerine kullanılır.", "csharp", """
            // Query string'den sayfa numarası
            if (!int.TryParse(Request.Query["sayfa"], out var sayfa) || sayfa < 1)
                sayfa = 1;

            // Tarih
            if (DateTime.TryParse(girdi, out var tarih))
                Console.WriteLine(tarih.ToShortDateString());
            else
                Console.WriteLine("Geçersiz tarih");
            """),

        ["using"] = T("Veritabanı bağlantısı, dosya, HTTP cevabı gibi kapatılması gereken kaynaklarda. Kapatılmayan bağlantılar zamanla 'connection pool' tükenmesine yol açar.", "csharp", """
            // VeritabaniServisi.cs — gerçek kod
            await using var komut = baglanti.CreateCommand();
            await using var okuyucu = await komut.ExecuteReaderAsync();
            // metot bitince ikisi de otomatik Dispose edilir

            // Program.cs — gerçek kod
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UygulamaDbContext>();
            """),

        ["LINQ"] = T("EF Core ile veritabanı sorgularının neredeyse tamamı LINQ ile yazılır; EF Core bunu SQL'e çevirir. Bellekteki listelerde de filtreleme/sıralama için kullanılır.", "csharp", """
            // AlanlarController.cs — gerçek kod
            var alanlar = await db.Alanlar
                .AsNoTracking()
                .OrderBy(a => a.Sira)
                .Select(a => new
                {
                    a.Id, a.Ad,
                    Toplam = a.Konular.Count,
                    Tamamlanan = a.Konular.Count(k => k.TamamlanmaTarihi != null),
                })
                .ToListAsync();
            // → SELECT a.Id, a.Ad, (SELECT COUNT(*) ...), (SELECT COUNT(*) ... WHERE ...) FROM Alanlar a ORDER BY a.Sira
            """),

        ["Lambda Expression"] = T("LINQ sorgularında, Minimal API endpoint'lerinde, olay (event) işleyicilerinde ve ayar kodlarında her yerde.", "csharp", """
            // LINQ koşulu
            db.Konular.Where(k => k.TamamlanmaTarihi == null)

            // Program.cs — DbContext ayarı
            builder.Services.AddDbContext<UygulamaDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Minimal API endpoint'i
            app.MapGet("/saglik", () => Results.Ok("çalışıyor"));
            """),

        ["async / await"] = T("Veritabanı, dosya ve ağ işlemi yapan her backend metodu async yazılır. Sunucunun aynı anda çok sayıda isteğe cevap verebilmesini sağlar.", "csharp", """
            // KonularController.cs — gerçek kod
            [HttpPost("{id:int}/tamamla")]
            public async Task<IActionResult> Tamamla(int id)
            {
                var bulundu = await ilerleme.KonuyuTamamlaAsync(id);   // veritabanını bekle
                return bulundu ? NoContent() : NotFound();
            }

            // ❌ Yapma: .Result veya .Wait() thread'i kilitler, kilitlenmeye (deadlock) yol açabilir
            var sonuc = ilerleme.KonuyuTamamlaAsync(id).Result;
            """),

        ["Task"] = T("Async metotların dönüş tipi. Birden fazla işi paralel başlatıp hepsini beklemek için Task.WhenAll kullanılır.", "csharp", """
            // İki bağımsız sorguyu sırayla değil, aynı anda çalıştır
            var alanTask = httpClient.GetFromJsonAsync<List<AlanDto>>("/api/alanlar");
            var projeTask = httpClient.GetFromJsonAsync<List<ProjeDto>>("/api/projeler");
            await Task.WhenAll(alanTask, projeTask);

            var alanlar = alanTask.Result;    // artık güvenli, iş bitti
            var projeler = projeTask.Result;
            // ⚠️ Aynı DbContext ile paralel sorgu ATILAMAZ; bu örnek HttpClient için
            """),

        ["Deferred Execution"] = T("EF Core sorgusu ToListAsync, FirstAsync, CountAsync çağrılana kadar veritabanına gitmez. Bu sayede koşullu filtreler adım adım eklenebilir.", "csharp", """
            // TerimlerController.cs — gerçek kod
            IQueryable<Terim> sorgu = db.Terimler.AsNoTracking();   // henüz SQL YOK

            if (!string.IsNullOrWhiteSpace(ara))
                sorgu = sorgu.Where(t => t.Ad.Contains(ara));       // hâlâ SQL yok

            var sonuc = await sorgu.OrderBy(t => t.Ad).ToListAsync(); // ŞİMDİ tek bir SQL gider
            """),

        // ======================= BACKEND =======================
        ["Client"] = T("Bir backend'in istemcileri: web sitesi (JavaScript), mobil uygulama, başka bir backend servisi, Postman/curl gibi araçlar.", "csharp", """
            // Başka bir .NET uygulamasından bizim API'ye istek (HttpClient istemcidir)
            using var http = new HttpClient { BaseAddress = new Uri("http://localhost:5019") };
            var bugun = await http.GetFromJsonAsync<BugunDto>("/api/bugun");
            Console.WriteLine(bugun?.Mesaj);

            // wwwroot/uygulama.js — tarayıcı istemcisi
            // const bugun = await fetch("/api/bugun").then(r => r.json());
            """),

        ["Server"] = T("ASP.NET Core uygulamaları Kestrel web sunucusu ile çalışır; canlı ortamda önüne genelde IIS veya Nginx konur.", "text", """
            > dotnet run
            info: Microsoft.Hosting.Lifetime[14]
                  Now listening on: http://localhost:5019     ← Kestrel bu portu dinliyor
            info: Microsoft.Hosting.Lifetime[0]
                  Application started. Press Ctrl+C to shut down.

            # Canlı ortam:  İnternet → IIS (443) → Kestrel → YolHaritasi.Api
            """),

        ["HTTP"] = T("Tüm web API'leri HTTP üzerinden konuşur. Bir isteği ham haliyle okuyabilmek, hata ayıklamanın temelidir.", "http", """
            GET /api/konular/1 HTTP/1.1
            Host: localhost:5019
            Accept: application/json

            HTTP/1.1 200 OK
            Content-Type: application/json; charset=utf-8

            {"id":1,"alanAdi":"C# Temelleri","baslik":"C#'a Giriş ve İlk Program", ...}
            """),

        ["Request / Response"] = T("Hata ayıklarken önce isteğin doğru gidip gitmediğine (metot, adres, gövde), sonra cevaba (durum kodu, gövde) bakarsın. Tarayıcıda F12 → Network.", "http", """
            ### İSTEK (request): metot + adres + başlıklar + gövde
            PATCH /api/gorevler/3 HTTP/1.1
            Content-Type: application/json

            {"tamamlandi": true}

            ### CEVAP (response): durum kodu + başlıklar (+ gövde)
            HTTP/1.1 204 No Content
            Date: Tue, 17 Nov 2026 12:00:00 GMT
            """),

        ["Status Code"] = T("API'nin sonucu anlatma biçimi. İstemci (mobil uygulama) davranışını buna göre belirler: 401 → giriş ekranına git, 404 → 'bulunamadı' göster.", "csharp", """
            // Bu projedeki controller'larda kullanılan durum kodları
            return Ok(veri);                    // 200 OK
            return CreatedAtAction(...);        // 201 Created (yeni kayıt oluşturuldu)
            return NoContent();                 // 204 No Content
            return BadRequest(new HataDto(...)); // 400 Bad Request
            return Unauthorized();              // 401 Unauthorized
            return StatusCode(403, ...);        // 403 Forbidden (VeritabaniController)
            return NotFound(new HataDto(...));  // 404 Not Found
            // Yakalanmayan exception → 500 Internal Server Error
            """),

        ["Port"] = T("Aynı sunucuda birden fazla uygulama çalışır; her biri farklı portu dinler. 'Port zaten kullanımda' hatası sık karşılaşılan bir sorundur.", "powershell", """
            # launchSettings.json → "applicationUrl": "http://localhost:5019"

            # Farklı portta çalıştırmak
            dotnet run --urls "http://localhost:6000"

            # "Address already in use" hatasında portu kimin kullandığını bul (Windows)
            netstat -ano | findstr :5019
            # Son sütundaki PID ile süreci bul/kapat
            Stop-Process -Id 12345

            # Bilinen portlar: 80 HTTP, 443 HTTPS, 1433 SQL Server, 3389 Uzak Masaüstü
            """),

        ["API"] = T("Mobil uygulama, web arayüzü ve diğer servisler veriye API üzerinden ulaşır. API'nin dokümanı (OpenAPI) diğer geliştiricilerin onu nasıl kullanacağını anlatır.", "text", """
            Bu projenin API'si (http://localhost:5019/scalar adresinde tamamı):

            GET    /api/bugun                    → bugünün konusu
            GET    /api/alanlar                  → alanlar ve ilerleme
            GET    /api/konular/{id}             → konu detayı
            POST   /api/konular/{id}/tamamla     → konuyu tamamla
            PATCH  /api/gorevler/{id}            → görev işaretle
            GET    /api/terimler?ara=...         → sözlük
            GET    /api/projeler                 → projeler
            POST   /api/veritabani/sorgu         → SQL çalıştır
            """),

        ["REST"] = T("Günümüz web API'lerinin çoğu REST tarzında tasarlanır. URL kaynağı, HTTP metodu işlemi anlatır.", "text", """
            Kaynak: kitaplar

            GET     /api/kitaplar          → listele         200
            GET     /api/kitaplar/5        → tek kitap       200 / 404
            POST    /api/kitaplar          → oluştur         201 + Location: /api/kitaplar/6
            PUT     /api/kitaplar/5        → tamamen güncelle 204
            PATCH   /api/kitaplar/5        → kısmen güncelle  204
            DELETE  /api/kitaplar/5        → sil             204

            İç içe kaynak: GET /api/yazarlar/3/kitaplar → 3 numaralı yazarın kitapları
            ❌ REST'e uymayan: GET /api/kitapSil?id=5
            """),

        ["Endpoint"] = T("API'ye yeni bir özellik eklemek genelde yeni bir endpoint yazmak demektir. Her endpoint bir controller metodudur.", "csharp", """
            // ProjelerController.cs — gerçek kod
            [ApiController]
            [Route("api/[controller]")]          // → /api/projeler
            public class ProjelerController(UygulamaDbContext db) : ControllerBase
            {
                [HttpGet]                            // GET   /api/projeler
                public async Task<List<ProjeDto>> Listele() { ... }

                [HttpPatch("adimlar/{id:int}")]      // PATCH /api/projeler/adimlar/12
                public async Task<IActionResult> AdimDurumu(int id, [FromBody] AdimDurumIstegi istek) { ... }
            }
            """),

        ["HTTP Methods"] = T("Doğru metodu seçmek API'nin anlaşılır olmasını sağlar. Veri değiştiren işlemler asla GET ile yapılmaz (tarayıcı ve arama motorları GET linklerini kendiliğinden çağırabilir).", "csharp", """
            [HttpGet]                       // okuma: güvenli, tekrar edilebilir
            [HttpPost]                      // oluşturma veya işlem (tamamla, gönder)
            [HttpPut("{id:int}")]           // kaydın tamamını değiştir
            [HttpPatch("{id:int}")]         // kaydın bir kısmını değiştir (GorevlerController)
            [HttpDelete("{id:int}")]        // sil
            """),

        ["JSON"] = T("API'lerin standart veri biçimi. C# nesneleri otomatik JSON'a çevrilir; property adları camelCase olur.", "json", """
            // GET /api/projeler cevabının bir parçası
            {
              "id": 1,
              "alanAdi": "C# Temelleri",
              "baslik": "Konsol Görev Yöneticisi",
              "zorluk": "Kolay",
              "adimSayisi": 7,
              "tamamlananAdimSayisi": 2,
              "adimlar": [
                { "id": 1, "sira": 1, "aciklama": "dotnet new console ...", "ipucu": null, "tamamlandi": true }
              ]
            }
            """),

        ["Serialization"] = T("ASP.NET Core bunu otomatik yapar, ama dosyaya kaydetme, önbelleğe yazma, kuyruk mesajı gönderme gibi durumlarda elle de yaparsın.", "csharp", """
            using System.Text.Json;

            var gorevler = new List<Gorev> { new() { Id = 1, Aciklama = "Test" } };

            // Serialization: nesne → JSON
            string json = JsonSerializer.Serialize(gorevler);
            File.WriteAllText("gorevler.json", json);

            // Deserialization: JSON → nesne
            var okunan = JsonSerializer.Deserialize<List<Gorev>>(File.ReadAllText("gorevler.json"));
            """),

        ["Query String"] = T("Filtreleme, arama, sıralama ve sayfalama parametreleri için kullanılır. Kaynağın kimliği ise route'ta olur (/api/konular/5).", "csharp", """
            // TerimlerController.cs — gerçek kod
            [HttpGet]                                     // GET /api/terimler?ara=key
            public async Task<List<TerimAramaDto>> Ara([FromQuery] string? ara)

            // Sayfalama örneği: GET /api/konular?alanId=2&sayfa=3&boyut=10
            public async Task<List<KonuOzetDto>> Listele(
                [FromQuery] int? alanId, [FromQuery] int sayfa = 1, [FromQuery] int boyut = 20)
                => await db.Konular.Skip((sayfa - 1) * boyut).Take(boyut)...;
            """),

        ["Dependency Injection (DI)"] = T("ASP.NET Core'da her şey DI ile bağlanır: DbContext, logger, ayarlar, HttpClient ve kendi servislerin.", "csharp", """
            // 1) Kaydet — Program.cs (gerçek kod)
            builder.Services.AddDbContext<UygulamaDbContext>(...);
            builder.Services.AddScoped<IIlerlemeServisi, IlerlemeServisi>();
            builder.Services.AddScoped<VeritabaniServisi>();

            // 2) İste — constructor'da (gerçek kod)
            public class VeritabaniController(VeritabaniServisi servis, IWebHostEnvironment ortam)

            // 3) Framework her istek için nesneleri oluşturup verir; sen hiç "new" yazmazsın
            """),

        ["Service Lifetime"] = T("Yanlış ömür seçimi gerçek hatalara yol açar: DbContext'i Singleton yapmak, aynı anda gelen isteklerin aynı bağlantıyı kullanıp çakışmasına neden olur.", "csharp", """
            builder.Services.AddSingleton<IOnbellek, BellekOnbellegi>();   // tek nesne, herkes paylaşır
            builder.Services.AddScoped<IIlerlemeServisi, IlerlemeServisi>(); // her HTTP isteğine bir tane
            builder.Services.AddTransient<IEpostaSablonu, EpostaSablonu>(); // her istendiğinde yeni

            // ❌ Hata: Singleton, Scoped'u constructor'dan alamaz
            // "Cannot consume scoped service 'UygulamaDbContext' from singleton 'Hatirlatici'"
            // ✅ Çözüm: IServiceScopeFactory ile scope aç (Program.cs'teki CreateScope gibi)
            """),

        ["Controller"] = T("Her kaynak (konular, görevler, projeler) için bir controller olur. İş kuralı servise, veri erişimi DbContext'e bırakılır; controller ince kalır.", "csharp", """
            // KonularController.cs — gerçek kod (ince controller)
            [ApiController]
            [Route("api/[controller]")]
            public class KonularController(IIlerlemeServisi ilerleme) : ControllerBase
            {
                [HttpGet("{id:int}")]
                public async Task<ActionResult<KonuDetayDto>> Getir(int id)
                {
                    var konu = await ilerleme.KonuDetayAsync(id);   // işi servise ver
                    return konu is null ? NotFound(...) : konu;     // sonucu HTTP'ye çevir
                }
            }
            """),

        ["Service Layer"] = T("İş kurallarını HTTP'den bağımsız tutar; aynı kural API'den, arka plan servisinden veya testlerden çağrılabilir.", "csharp", """
            // IlerlemeServisi.cs — gerçek iş kuralı: "Bugünün konusu sıradaki ilk tamamlanmamış konudur"
            var siradakiId = await db.Konular
                .Where(k => k.TamamlanmaTarihi == null)
                .OrderBy(k => k.Alan.Sira).ThenBy(k => k.Sira)
                .Select(k => (int?)k.Id)
                .FirstOrDefaultAsync();

            // Aynı servis ileride Windows Service'ten de çağrılacak:
            // var bugun = await ilerlemeServisi.BugunAsync();  → "Bugün LINQ çalışmalısın" bildirimi
            """),

        ["DTO"] = T("Entity'yi dışarı açmadan API cevabını şekillendirmek, istek gövdesini tanımlamak, hassas alanları (şifre hash'i) gizlemek için.", "csharp", """
            // Entity (veritabanı şekli)            // DTO (API şekli)
            public class Konu                        public record KonuOzetDto(
            {                                            int Id,
                public int Id { get; set; }              int Sira,
                public int AlanId { get; set; }          string Baslik,
                public Alan Alan { get; set; }           string Ozet,
                public List<Terim> Terimler ...          int TahminiDakika,
                public string? DersNotu ...              bool Tamamlandi);   // hesaplanmış alan
                ...
            }
            // Dönüşüm: .Select(k => new KonuOzetDto(k.Id, k.Sira, ..., k.TamamlanmaTarihi != null))
            """),

        ["Separation of Concerns"] = T("Kodun büyüdükçe yönetilebilir kalması için. Bir değişiklik tek bir yeri etkilemeli: tablo değişince controller, tasarım değişince servis değişmemeli.", "text", """
            Bu projede her sorumluluk ayrı bir yerde:

            wwwroot/uygulama.js        → ekranı çizmek
            Controllers/               → HTTP isteğini karşılamak
            Services/                  → iş kuralları
            Data/UygulamaDbContext.cs  → veritabanı eşlemesi
            Entities/                  → tablo yapısı
            Dtos/                      → API sözleşmesi
            Data/BaslangicVerisi*.cs   → içerik
            """),

        ["Authentication"] = T("Kullanıcıya özel veri sunan her uygulamada gerekir: giriş, kayıt, şifre sıfırlama, oturum yönetimi.", "csharp", """
            // Giriş endpoint'i (Adım 3'te ekleyeceğiz — taslak)
            [HttpPost("giris")]
            public async Task<IActionResult> Giris(GirisIstegi istek)
            {
                var kullanici = await db.Kullanicilar.SingleOrDefaultAsync(k => k.Email == istek.Email);
                if (kullanici is null || !sifreServisi.Dogrula(istek.Sifre, kullanici.SifreHash))
                    return Unauthorized(new HataDto("E-posta veya şifre hatalı."));

                return Ok(new { token = tokenServisi.Uret(kullanici) });
            }
            """),

        ["Authorization"] = T("Kim neyi yapabilir: admin içerik ekler, kullanıcı sadece kendi ilerlemesini değiştirir.", "csharp", """
            [Authorize]                                  // giriş zorunlu
            public class IlerlemeController : ControllerBase { }

            [Authorize(Roles = "Admin")]                 // sadece admin
            [HttpPost("api/konular")]
            public async Task<IActionResult> Ekle(...) { }

            [AllowAnonymous]                             // herkese açık
            [HttpGet("api/terimler")]
            public async Task<List<TerimAramaDto>> Ara(...) { }

            // Program.cs: app.UseAuthentication(); app.UseAuthorization();  (bu sırayla!)
            """),

        ["Hash"] = T("Şifreler, API anahtarları, dosya bütünlük kontrolleri. Veritabanında şifre ASLA düz metin durmaz.", "csharp", """
            using Microsoft.AspNetCore.Identity;

            var hasher = new PasswordHasher<Kullanici>();

            // Kayıt: hash'le ve sakla
            kullanici.SifreHash = hasher.HashPassword(kullanici, "Gizli123!");
            // → "AQAAAAIAAYagAAAAE...uzun bir metin..." (her seferinde farklı, çünkü salt içerir)

            // Giriş: doğrula
            var sonuc = hasher.VerifyHashedPassword(kullanici, kullanici.SifreHash, girilenSifre);
            bool dogru = sonuc == PasswordVerificationResult.Success;
            """),

        ["JWT"] = T("Mobil uygulamalar ve SPA'lar (tek sayfalı web uygulamaları) API'ye kimliklerini JWT ile kanıtlar. Giriş sonrası alınır, her istekte gönderilir.", "http", """
            ### 1) Giriş yap, token al
            POST /api/hesap/giris
            Content-Type: application/json

            {"email": "emre@ornek.com", "sifre": "..."}

            → 200 OK  {"token": "eyJhbGciOiJIUzI1NiIs..."}

            ### 2) Token ile korumalı endpoint'i çağır
            GET /api/ilerlemem
            Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

            → 200 OK (token yoksa/geçersizse 401 Unauthorized)
            """),

        ["Middleware"] = T("Her isteğe uygulanacak ortak işler için: loglama, hata yakalama, kimlik doğrulama, CORS, sıkıştırma, istek süresi ölçme.", "csharp", """
            // Program.cs'e eklenebilecek basit bir middleware: yavaş istekleri logla
            app.Use(async (context, next) =>
            {
                var sayac = System.Diagnostics.Stopwatch.StartNew();
                await next();   // isteği sonraki adıma ilet
                if (sayac.ElapsedMilliseconds > 500)
                    app.Logger.LogWarning("Yavaş istek: {Yol} {Sure} ms",
                        context.Request.Path, sayac.ElapsedMilliseconds);
            });
            """),

        ["Pipeline"] = T("Middleware'lerin sırası davranışı belirler. Yanlış sıra, sessiz hatalara yol açar (ör. yetki kontrolünün hiç çalışmaması).", "csharp", """
            // Program.cs — gerçek sıra
            app.MapOpenApi();            // /openapi/v1.json
            app.UseHttpsRedirection();   // http → https
            app.UseDefaultFiles();       // / → index.html
            app.UseStaticFiles();        // wwwroot dosyaları (bulursa burada biter)
            app.MapControllers();        // /api/... → controller

            // Kimlik doğrulama eklenince:
            // app.UseAuthentication();   önce
            // app.UseAuthorization();    sonra
            """),

        ["Logging"] = T("Canlı ortamda hata ayıklamanın tek yolu loglardır. Önemli işlemler Information, beklenmeyen durumlar Warning, hatalar Error seviyesinde yazılır.", "csharp", """
            // IlerlemeServisi.cs — gerçek kod (structured logging)
            logger.LogInformation("Konu tamamlandı. KonuId={KonuId}, Baslik={Baslik}", konu.Id, konu.Baslik);

            // VeritabaniServisi.cs — gerçek kod (hata + exception)
            logger.LogError(ex, "Veritabanı durumu okunamadı");

            // appsettings.json: hangi seviyeden itibaren yazılsın
            // "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
            """),
    };
}
