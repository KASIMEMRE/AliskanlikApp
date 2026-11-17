namespace YolHaritasi.Api.Data;

// ============================================================================
//  DERS NOTLARI: Konuların Türkçe anlatımı (Markdown biçiminde).
//  Anahtar: (Alan adı, Konu sırası). Örn: ("C# Temelleri", 7) → OOP: Sınıf ve Nesne
//
//  """ ... """ → C# 11 RAW STRING LITERAL: İçindeki tırnak ve ters bölü işaretleri
//  kaçış karakteri gerektirmeden olduğu gibi yazılır. Uzun metinler için idealdir.
// ============================================================================
public static partial class BaslangicVerisi
{
    private static readonly Dictionary<(string Alan, int Sira), string> DersNotlari = new()
    {
        // ===================================================================
        //  C# TEMELLERİ
        // ===================================================================
        [("C# Temelleri", 1)] = """
## C# nedir?
C# (si-şarp), Microsoft'un geliştirdiği, **nesne yönelimli** ve **tip güvenli** bir programlama dilidir. Web API'ler, masaüstü uygulamaları, mobil uygulamalar (MAUI), oyunlar (Unity) ve bulut servisleri C# ile yazılabilir.

## C# ve .NET ilişkisi
- **C#** dildir: Kodu yazdığımız kurallar.
- **.NET** platformdur: Kodu derleyen araçlar, çalıştıran ortam (runtime) ve hazır kütüphaneler.

Kodun çalışma yolu şöyle:
1. Sen `.cs` dosyasına C# yazarsın.
2. **Derleyici** bunu **IL** (Intermediate Language, ara dil) koduna çevirir ve bir `.dll` dosyası üretir.
3. Program çalışırken .NET runtime, IL kodunu o bilgisayarın işlemcisine uygun makine koduna çevirir (**JIT**: Just-In-Time derleme).

Bu sayede aynı `.dll` Windows'ta da Linux'ta da çalışabilir.

## İlk program
```csharp
// Program.cs
Console.WriteLine("Merhaba Dünya!");

var ad = "Emre";
Console.WriteLine($"Merhaba {ad}, bugün {DateTime.Now:dd.MM.yyyy}");
```
Terminalde:
```
dotnet new console -n Merhaba
cd Merhaba
dotnet run
```

## Top-level statements
Eski C# sürümlerinde her program şöyle başlardı:
```csharp
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Merhaba");
    }
}
```
Yeni sürümlerde bu kalıbı derleyici bizim yerimize yazıyor; doğrudan kodla başlayabiliyoruz. Bizim API projemizin `Program.cs` dosyası da böyle başlıyor.

## Yorum satırları
```csharp
// Tek satırlık yorum

/* Çok satırlı
   yorum */

/// <summary>XML dokümantasyon yorumu: API arayüzünde açıklama olarak görünür</summary>
```
""",

        [("C# Temelleri", 2)] = """
## Değişken nedir?
Değişken, bir değeri bellekte saklamak için verdiğimiz isimdir. C#'ta her değişkenin bir **tipi** vardır ve bu tip sonradan değişmez (**statik tipli dil**).

```csharp
int yas = 25;              // tam sayı
double boy = 1.78;         // ondalıklı sayı
decimal fiyat = 199.90m;   // para hesapları için (hassas)
bool aktifMi = true;       // true / false
char harf = 'A';           // tek karakter (tek tırnak)
string ad = "Emre";        // metin (çift tırnak)
DateTime simdi = DateTime.Now;
```

## Hangi sayı tipi ne zaman?
- `int`: Sayaç, kimlik (Id), adet. Bizim tablolarda `Id` sütunları int.
- `long`: Çok büyük tam sayılar.
- `double`: Bilimsel hesaplar. Küçük yuvarlama hataları olabilir.
- `decimal`: **Para.** Yuvarlama hatası olmaması gereken her yerde.

## var anahtar kelimesi
```csharp
var sehir = "İstanbul";   // derleyici bunun string olduğunu anlar
sehir = 34;               // HATA! Tip hâlâ string
```
`var` tipi yok etmez, sadece yazmayı kısaltır.

## Tip dönüşümü (casting)
```csharp
int a = 10;
double b = a;              // otomatik (implicit): küçük tipten büyüğe güvenli

double c = 9.7;
int d = (int)c;            // açık (explicit): d = 9, ondalık kısım kaybolur

int e = int.Parse("42");   // metinden sayıya
string f = e.ToString();   // sayıdan metne
```

## Kullanıcıdan veri almak
```csharp
Console.Write("Yaşın: ");
string? girdi = Console.ReadLine();
int yas = int.Parse(girdi!);
Console.WriteLine($"10 yıl sonra {yas + 10} yaşında olacaksın.");
```

## Nullable (soru işaretli) tipler
`string?` "bu değişken null olabilir" demektir. Projemizde `Konu.TamamlanmaTarihi` bir `DateTime?`: konu tamamlanmadıysa değeri yok (veritabanında NULL).
""",

        [("C# Temelleri", 3)] = """
## Aritmetik operatörler
```csharp
int a = 17, b = 5;
Console.WriteLine(a + b);   // 22
Console.WriteLine(a - b);   // 12
Console.WriteLine(a * b);   // 85
Console.WriteLine(a / b);   // 3   ← iki int bölünürse sonuç int! (ondalık atılır)
Console.WriteLine(a % b);   // 2   ← mod: bölümden kalan
Console.WriteLine(17.0 / 5); // 3.4
```

## Karşılaştırma ve mantıksal operatörler
```csharp
bool buyukMu = a > b;                    // true
bool esitMi = a == b;                    // false  (== karşılaştırma, = atama!)
bool ikisi = a > 10 && b > 10;           // VE  → false
bool biri  = a > 10 || b > 10;           // VEYA → true
bool degil = !buyukMu;                   // DEĞİL → false
```

## String işlemleri
```csharp
string ad = "  Yol Haritası  ";
ad.Length;              // 16 (boşluklar dahil)
ad.Trim();              // "Yol Haritası"
ad.ToUpper();           // "  YOL HARİTASI  "
ad.Contains("Harita");  // true
ad.Replace("Yol", "Yön");
ad.Split(' ');          // boşluklardan böl → dizi
```

## String interpolation
```csharp
var konu = "LINQ";
var dakika = 90;
Console.WriteLine($"{konu} konusu yaklaşık {dakika} dakika sürer.");
Console.WriteLine($"Yüzde: {0.456:P0}");   // biçimlendirme: %46
```

## Math sınıfı
```csharp
Math.Max(3, 8);      // 8
Math.Round(2.567, 2); // 2.57
Math.Sqrt(16);       // 4
Math.Abs(-5);        // 5
```

## Projede nerede?
`AlanlarController.cs` içinde ilerleme yüzdesi şöyle hesaplanıyor:
```csharp
a.Toplam == 0 ? 0 : a.Tamamlanan * 100 / a.Toplam
```
Burada `? :` **üçlü (ternary) operatör**: "Koşul doğruysa ilki, değilse ikincisi". Toplam 0 ise sıfıra bölme hatasını önlüyor.
""",

        [("C# Temelleri", 4)] = """
## if / else
```csharp
int puan = 72;

if (puan >= 85)
    Console.WriteLine("Pekiyi");
else if (puan >= 50)
    Console.WriteLine("Geçti");
else
    Console.WriteLine("Kaldı");
```

## switch expression (modern yazım)
```csharp
string harf = puan switch
{
    >= 85 => "A",
    >= 70 => "B",
    >= 50 => "C",
    _     => "F"      // _ = "diğer her şey"
};
```

## for döngüsü
Kaç kez döneceğin belliyse:
```csharp
for (int i = 1; i <= 5; i++)
{
    Console.WriteLine($"{i}. tekrar");
}
```
Üç parça: başlangıç (`int i = 1`), devam koşulu (`i <= 5`), her turdan sonra (`i++`).

## while döngüsü
Koşul doğru olduğu sürece:
```csharp
int deneme = 0;
while (deneme < 3)
{
    Console.WriteLine("Şifre?");
    deneme++;
}
```

## foreach döngüsü
Bir koleksiyonun her elemanı için. Backend'de en çok bunu kullanırsın:
```csharp
var konular = new List<string> { "HTTP", "REST", "DI" };
foreach (var konu in konular)
{
    Console.WriteLine(konu);
}
```

## break ve continue
```csharp
foreach (var sayi in sayilar)
{
    if (sayi < 0) continue;   // negatifi atla, sonrakine geç
    if (sayi > 100) break;    // döngüyü tamamen bitir
    Console.WriteLine(sayi);
}
```

## Sık hata: sonsuz döngü
`while` içinde koşulu değiştiren satırı (`deneme++`) unutursan döngü hiç bitmez. Program donar; `Ctrl+C` ile durdurursun.
""",

        [("C# Temelleri", 5)] = """
## Array (dizi)
Boyutu sabittir; oluşturulduktan sonra eleman eklenemez.
```csharp
int[] notlar = [70, 85, 90];
Console.WriteLine(notlar[0]);      // 70  (indeks 0'dan başlar)
Console.WriteLine(notlar.Length);  // 3
notlar[3] = 50;                    // HATA: IndexOutOfRangeException
```

## List<T>
Boyutu büyüyüp küçülebilir. **Backend'de en çok kullanılan koleksiyon.**
```csharp
var konular = new List<string>();
konular.Add("HTTP");
konular.Add("REST");
konular.Insert(0, "C#");     // başa ekle
konular.Remove("REST");
Console.WriteLine(konular.Count);       // 2
Console.WriteLine(konular.Contains("HTTP")); // true
```
`<T>` bir **generic** parametredir: `List<int>`, `List<Konu>`, `List<TerimDto>`... Aynı List sınıfı her tiple çalışır ve tip güvenliğini korur. `List<int>` içine string ekleyemezsin.

## Dictionary<TKey, TValue>
Anahtarla değere çok hızlı ulaşmak için:
```csharp
var sozluk = new Dictionary<string, string>
{
    ["API"] = "Uygulama programlama arayüzü",
    ["SQL"] = "Yapılandırılmış sorgu dili",
};

sozluk["DTO"] = "Veri aktarım nesnesi";      // ekle veya güncelle

if (sozluk.TryGetValue("API", out var anlam))
    Console.WriteLine(anlam);
```

## Projede nerede?
- `Alan.Konular` bir `List<Konu>`: Bir alanın konuları.
- Ders notları (şu an okuduğun metin) bir `Dictionary<(string, int), string>` içinde duruyor: Anahtar (Alan adı, konu sırası). `BaslangicVerisi.DersNotlari.cs` dosyasına bak.
""",

        [("C# Temelleri", 6)] = """
## Metot nedir?
Bir işi yapan, adı olan ve tekrar tekrar çağrılabilen kod bloğudur.

```csharp
//  dönüş tipi   ad     parametreler
static double Ortalama(List<int> notlar)
{
    if (notlar.Count == 0) return 0;
    return notlar.Sum() / (double)notlar.Count;
}

var sonuc = Ortalama([70, 85, 90]);   // çağırma: argüman veriyoruz
```

## void
Bir şey döndürmeyen metot:
```csharp
static void Selamla(string ad)
{
    Console.WriteLine($"Merhaba {ad}");
}
```

## Varsayılan ve isimli parametreler
```csharp
static string Bicimle(string metin, bool buyukHarf = false) =>
    buyukHarf ? metin.ToUpper() : metin;

Bicimle("api");                    // "api"
Bicimle("api", buyukHarf: true);   // "API"
```

## Overloading (aşırı yükleme)
Aynı isim, farklı parametre listesi:
```csharp
static int Topla(int a, int b) => a + b;
static double Topla(double a, double b) => a + b;
static int Topla(int a, int b, int c) => a + b + c;
```

## out parametresi
Bir metottan birden fazla sonuç almanın yolu. `int.TryParse` bunu kullanır:
```csharp
if (int.TryParse("42", out int sayi))
    Console.WriteLine(sayi);
```

## Neden metotlara bölüyoruz?
- **Tekrarı önler** (DRY: Don't Repeat Yourself).
- **Okunabilirlik:** `KonuyuTamamlaAsync(5)` satırı ne yaptığını adıyla söyler.
- **Test edilebilirlik:** Küçük metotları ayrı ayrı test edebilirsin.

Projemizde `IlerlemeServisi` içindeki her iş ayrı bir metot: `BugunAsync`, `KonuDetayAsync`, `KonuyuTamamlaAsync`.
""",

        [("C# Temelleri", 7)] = """
## Sınıf ve nesne
**Sınıf** bir kalıptır (ev planı), **nesne** o kalıptan üretilmiş somut örnektir (planla yapılmış ev).

```csharp
public class Konu
{
    // PROPERTY'ler: nesnenin verisi
    public int Id { get; set; }
    public string Baslik { get; set; } = "";
    public int TahminiDakika { get; set; }

    // METOT: nesnenin davranışı
    public bool UzunMu() => TahminiDakika > 60;
}

var k1 = new Konu { Id = 1, Baslik = "HTTP", TahminiDakika = 60 };
var k2 = new Konu { Id = 2, Baslik = "LINQ", TahminiDakika = 90 };
Console.WriteLine(k2.UzunMu());   // True
```
`k1` ve `k2` aynı sınıftan üretilmiş **iki ayrı nesne**dir.

## Constructor (yapıcı metot)
Nesne `new` ile oluşturulurken çalışır:
```csharp
public class BankaHesabi
{
    public string Sahip { get; }
    public decimal Bakiye { get; private set; }   // dışarıdan okunur, sadece içeriden değiştirilir

    public BankaHesabi(string sahip, decimal ilkBakiye)
    {
        Sahip = sahip;
        Bakiye = ilkBakiye;
    }

    public void ParaCek(decimal tutar)
    {
        if (tutar > Bakiye)
            throw new InvalidOperationException("Yetersiz bakiye");
        Bakiye -= tutar;
    }
}
```

## Primary constructor (C# 12)
Projemizdeki controller'larda gördüğün kısa yazım:
```csharp
public class KonularController(IIlerlemeServisi ilerleme) : ControllerBase
{
    // "ilerleme" sınıfın her yerinde kullanılabilir
}
```
Bu, constructor yazıp parametreyi bir alana atamanın kısa yoludur.

## Access modifier'lar
- `public`: Her yerden erişilir.
- `private`: Sadece sınıfın kendi içinden.
- `protected`: Sınıfın kendisi ve ondan türeyen sınıflar.
- `internal`: Aynı proje (assembly) içinden.

## Encapsulation (kapsülleme)
`BankaHesabi` örneğinde `Bakiye` dışarıdan doğrudan değiştirilemiyor (`private set`). Değiştirmenin tek yolu `ParaCek` metodu, o da kuralı (yetersiz bakiye) kontrol ediyor. Verinin kurallara aykırı bir duruma düşmesini engellemek OOP'nin temel fikridir.

## Projede nerede?
`Entities/` klasöründeki her dosya bir sınıf. EF Core her `Konu` nesnesini `Konular` tablosunda bir **satıra**, her property'yi bir **sütuna** eşliyor.
""",

        [("C# Temelleri", 8)] = """
## Kalıtım (inheritance)
Bir sınıf başka bir sınıfın üyelerini devralır:
```csharp
public class Kisi
{
    public string Ad { get; set; } = "";
    public virtual string Tanit() => $"Ben {Ad}";
}

public class Ogrenci : Kisi          // Ogrenci, Kisi'den türer
{
    public int Seviye { get; set; }
    public override string Tanit() => $"Ben {Ad}, seviye {Seviye} öğrenciyim";
}
```
`virtual` "alt sınıf bunu değiştirebilir", `override` "değiştiriyorum" demektir.

Projede: `KonularController : ControllerBase`. Controller'ımız ASP.NET Core'un `ControllerBase` sınıfından türüyor. `NotFound()`, `NoContent()` gibi metotlar oradan geliyor.

## Interface (arayüz)
"Ne yapılacağını" söyler, "nasıl yapılacağını" söylemez:
```csharp
public interface IBildirimGonderici
{
    void Gonder(string mesaj);
}

public class EpostaGonderici : IBildirimGonderici
{
    public void Gonder(string mesaj) => Console.WriteLine($"E-posta: {mesaj}");
}

public class SmsGonderici : IBildirimGonderici
{
    public void Gonder(string mesaj) => Console.WriteLine($"SMS: {mesaj}");
}
```

## Polimorfizm (çok biçimlilik)
```csharp
List<IBildirimGonderici> gondericiler = [new EpostaGonderici(), new SmsGonderici()];

foreach (var g in gondericiler)
    g.Gonder("Bugünün konusu: LINQ");   // her biri kendi yöntemiyle gönderir
```
Kod `g`'nin gerçekte ne olduğunu bilmiyor, bilmesine de gerek yok. Sadece "gönderebildiğini" biliyor.

## Backend'de neden bu kadar önemli?
Projemizde:
```csharp
public interface IIlerlemeServisi { Task<BugunDto> BugunAsync(); ... }
public class IlerlemeServisi : IIlerlemeServisi { ... }

// Program.cs
builder.Services.AddScoped<IIlerlemeServisi, IlerlemeServisi>();
```
Controller sadece `IIlerlemeServisi`'ni tanır. Yarın veritabanı yerine başka bir kaynaktan veri getiren bir sınıf yazsak, controller'a hiç dokunmadan sadece `Program.cs`'teki bu satırı değiştiririz. Test yazarken de sahte bir sınıf veririz. Bu **Dependency Injection**'ın temelidir.

## record
Veri taşımak için kısa yazım:
```csharp
public record TerimDto(int Id, string Ad, string Tanim);

var a = new TerimDto(1, "API", "...");
var b = new TerimDto(1, "API", "...");
Console.WriteLine(a == b);   // True: record'lar DEĞERLERİNE göre karşılaştırılır
```
Projedeki tüm DTO'lar (`Dtos/Dtolar.cs`) record.

## enum
```csharp
enum Zorluk { Kolay, Orta, Zor }
var z = Zorluk.Orta;
```
""",

        [("C# Temelleri", 9)] = """
## Exception nedir?
Program çalışırken beklenmeyen bir durum olduğunda .NET bir **exception** (istisna) nesnesi fırlatır. Yakalanmazsa program durur.

```csharp
int sayi = int.Parse("abc");   // FormatException → program çöker
```

## try / catch / finally
```csharp
try
{
    int sayi = int.Parse(girdi);
    Console.WriteLine(100 / sayi);
}
catch (FormatException)
{
    Console.WriteLine("Lütfen sayı gir.");
}
catch (DivideByZeroException)
{
    Console.WriteLine("Sıfıra bölünemez.");
}
catch (Exception ex)          // diğer her şey (en sona yazılır)
{
    Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
}
finally
{
    Console.WriteLine("Bu satır her durumda çalışır.");
}
```

## TryParse: Exception'sız yol
Hata bekleniyorsa (kullanıcı girdisi gibi) exception yerine bunu kullan; hem daha hızlı hem daha temiz:
```csharp
if (int.TryParse(girdi, out var yas))
    Console.WriteLine($"Yaş: {yas}");
else
    Console.WriteLine("Geçersiz sayı");
```

## throw: Bilerek hata fırlatmak
```csharp
public void ParaYatir(decimal tutar)
{
    if (tutar <= 0)
        throw new ArgumentException("Tutar pozitif olmalı", nameof(tutar));
    Bakiye += tutar;
}
```

## Dosya işlemleri
```csharp
// Yazma
File.WriteAllText("not.txt", "Bugün LINQ çalıştım");
File.AppendAllText("not.txt", "\nYarın async");
File.WriteAllLines("konular.txt", ["HTTP", "REST", "DI"]);

// Okuma
string metin = File.ReadAllText("not.txt");
string[] satirlar = File.ReadAllLines("konular.txt");

// Var mı?
if (File.Exists("not.txt")) { ... }
```

## using: kaynakları kapatmak
Dosya, veritabanı bağlantısı gibi kaynaklar işi bitince kapatılmalıdır:
```csharp
using var yazici = new StreamWriter("log.txt");
yazici.WriteLine("Kayıt");
// blok bitince yazici otomatik kapatılır (Dispose)
```
Projede `Program.cs` içindeki `using var scope = app.Services.CreateScope();` satırı aynı mantıkla çalışıyor.

## Backend'de exception
API'de yakalanmayan bir exception, kullanıcıya **500 Internal Server Error** olarak döner. İleride tüm hataları tek bir yerde yakalayan bir **middleware** yazacağız (Backend → 6. konu).
""",

        [("C# Temelleri", 10)] = """
## LINQ nedir?
**Language Integrated Query**: C# içinde koleksiyonlar üzerinde sorgu yazma yöntemi. SQL'e benzer ama C# kodudur.

```csharp
var konular = new List<(string Ad, int Dakika)>
{
    ("HTTP", 60), ("REST", 90), ("DI", 90), ("LINQ", 120)
};

var uzunlar = konular
    .Where(k => k.Dakika >= 90)         // filtrele      (SQL: WHERE)
    .OrderByDescending(k => k.Dakika)   // sırala        (SQL: ORDER BY ... DESC)
    .Select(k => k.Ad)                  // dönüştür      (SQL: SELECT Ad)
    .ToList();                          // çalıştır ve listeye çevir
// ["LINQ", "REST", "DI"]
```

## En çok kullanılan LINQ metotları
- `Where(koşul)`: Filtrele.
- `Select(dönüşüm)`: Her elemanı başka bir şeye çevir (projeksiyon).
- `OrderBy` / `OrderByDescending` / `ThenBy`: Sırala.
- `First` / `FirstOrDefault`: İlk eleman (yoksa hata / yoksa null).
- `Any` / `All`: Hiç var mı? / Hepsi mi?
- `Count` / `Sum` / `Average` / `Max`: Topla, say.
- `GroupBy`: Grupla.

## Lambda
`k => k.Dakika >= 90` bir **lambda** ifadesidir: "k verilirse, k.Dakika >= 90 sonucunu döndür". İsimsiz, kısa bir fonksiyon.

## LINQ + EF Core = SQL
Projemizde LINQ veritabanı üzerinde yazılıyor ve **EF Core onu SQL'e çeviriyor**:
```csharp
await db.Konular
    .Where(k => k.TamamlanmaTarihi == null)
    .OrderBy(k => k.Alan.Sira).ThenBy(k => k.Sira)
    .Select(k => k.Id)
    .FirstOrDefaultAsync();
```
Bu sorgu SQL Server'a şöyle gider:
```
SELECT TOP(1) k.Id FROM Konular k
INNER JOIN Alanlar a ON k.AlanId = a.Id
WHERE k.TamamlanmaTarihi IS NULL
ORDER BY a.Sira, k.Sira
```
Uygulamayı çalıştırdığında terminalde bu SQL'leri görebilirsin.

## async / await
Veritabanı sorgusu, internet isteği, dosya okuma gibi işlemler **beklemek** gerektirir. Beklerken iş parçacığını (thread) boşta tutmak yerine serbest bırakırız; böylece sunucu o sırada başka isteklere cevap verebilir.

```csharp
public async Task<List<string>> BaslikleriGetirAsync()
{
    var konular = await db.Konular.ToListAsync();   // burada "bekle", thread'i serbest bırak
    return konular.Select(k => k.Baslik).ToList();
}
```
Kurallar:
- `await` kullanan metot `async` olarak işaretlenir.
- `async` metot `Task` (sonuçsuz) veya `Task<T>` (sonuçlu) döndürür.
- İsimlendirme geleneği: async metotların sonu `Async` ile biter.

## Neden backend için kritik?
Bir web sunucusu aynı anda yüzlerce isteğe cevap verir. Her istek veritabanını beklerken bir thread'i kilitlerse thread'ler hızla tükenir ve sunucu yavaşlar. `async/await` ile aynı sayıda thread çok daha fazla isteği karşılayabilir.
""",

        // ===================================================================
        //  BACKEND
        // ===================================================================
        [("Backend", 1)] = """
## İstemci-sunucu modeli
Web'de iki taraf vardır:
- **İstemci (client):** İsteği başlatan taraf: tarayıcı, mobil uygulama, başka bir sunucu.
- **Sunucu (server):** İsteği karşılayıp cevap üreten program. Bizim `YolHaritasi.Api` bir sunucu.

Uygulamanın ekranı (wwwroot) da bir istemci. Her tıklamada API'ye istek gönderiyor.

## Bir HTTP isteğinin anatomisi
```
POST /api/gorevler/3 HTTP/1.1          ← metot, yol, protokol
Host: localhost:5019                   ← başlıklar (headers)
Content-Type: application/json

{ "tamamlandi": true }                 ← gövde (body)
```

## Bir HTTP cevabının anatomisi
```
HTTP/1.1 200 OK                        ← durum kodu
Content-Type: application/json         ← başlıklar

{ "id": 3, "baslik": "..." }           ← gövde
```

## Durum kodları
- **2xx Başarılı:** 200 OK, 201 Created, 204 No Content
- **3xx Yönlendirme:** 301 Moved Permanently, 302 Found
- **4xx İstemci hatası:** 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 429 Too Many Requests
- **5xx Sunucu hatası:** 500 Internal Server Error, 502 Bad Gateway, 503 Service Unavailable

Kural: 4xx gördüğünde "istekte bir sorun var", 5xx gördüğünde "sunucuda (bizim kodda) bir sorun var".

## Başlıklar (headers)
İstek ve cevapla ilgili ek bilgiler:
- `Content-Type`: Gövdenin biçimi (`application/json`, `text/html`)
- `Authorization`: Kimlik bilgisi (ileride JWT burada gidecek)
- `Accept`: İstemcinin kabul ettiği biçim

## Kendin gör
Uygulamayı aç, **F12 → Network** sekmesine geç, bir göreve tıkla. `PATCH` isteğini seç: Headers'ta metodu ve durum kodunu, Payload'da gönderilen gövdeyi göreceksin.
""",

        [("Backend", 2)] = """
## REST nedir?
REST, API tasarlamak için bir yaklaşımdır. Temel fikir: Her şey bir **kaynaktır** (resource) ve bir URL ile adreslenir. Üzerinde yapılacak işlemi HTTP metodu belirler.

## Kaynak + metot = işlem
```
GET    /api/konular        → konuları listele
GET    /api/konular/5      → 5 numaralı konuyu getir
POST   /api/konular        → yeni konu oluştur
PUT    /api/konular/5      → 5'i tamamen güncelle
PATCH  /api/konular/5      → 5'in bir kısmını güncelle
DELETE /api/konular/5      → 5'i sil
```
URL'de fiil değil, **isim** kullanılır: `/api/konuSil?id=5` değil, `DELETE /api/konular/5`.

## Bizim API'de
```csharp
[ApiController]
[Route("api/[controller]")]                  // → /api/konular
public class KonularController(...) : ControllerBase
{
    [HttpGet("{id:int}")]                    // GET /api/konular/5
    public async Task<ActionResult<KonuDetayDto>> Getir(int id) { ... }

    [HttpPost("{id:int}/tamamla")]           // POST /api/konular/5/tamamla
    public async Task<IActionResult> Tamamla(int id) { ... }
}
```
`tamamla` gibi kaynağa uymayan işlemler için `POST /kaynak/{id}/islem` kalıbı yaygın bir uzlaşmadır.

## Parametreler nereden gelir? (Model binding)
```csharp
[HttpGet("{id:int}")]  ... (int id)                     // route:  /api/konular/5
[HttpGet] ... ([FromQuery] string? ara)                 // query:  /api/terimler?ara=key
[HttpPatch("{id:int}")] ... ([FromBody] Istek istek)    // body:   { "tamamlandi": true }
```

## JSON ve serileştirme
API, C# nesnelerini otomatik olarak JSON'a çevirir (**serialization**):
```csharp
new TerimDto(1, "API", "Uygulama arayüzü", null)
```
şuna dönüşür:
```
{ "id": 1, "ad": "API", "tanim": "Uygulama arayüzü", "ornek": null }
```
Dikkat: C#'taki `PascalCase` property adları JSON'da `camelCase` oluyor. Bu ASP.NET Core'un varsayılan ayarı.

## Idempotent metotlar
GET, PUT ve DELETE **idempotent**tir: Aynı isteği 5 kez göndermek ile 1 kez göndermek aynı sonucu verir. POST genelde değildir (5 kez gönderirsen 5 kayıt oluşabilir).
""",

        [("Backend", 3)] = """
## Problem: Sıkı bağımlılık
```csharp
public class KonularController
{
    private readonly IlerlemeServisi _servis = new IlerlemeServisi(new UygulamaDbContext(...));
}
```
Bu kodda controller, servisin **nasıl oluşturulacağını** biliyor. Servisin constructor'ı değişirse controller da değişmek zorunda. Test için sahte servis vermek de imkânsız.

## Çözüm: Dependency Injection
Sınıf ihtiyacını constructor'da **ister**, kimin vereceğiyle ilgilenmez:
```csharp
public class KonularController(IIlerlemeServisi ilerleme) : ControllerBase { ... }
```
Nesneleri oluşturmak **DI konteynerinin** işidir. Ona ne zaman neyi vereceğini `Program.cs`'te söyleriz:
```csharp
builder.Services.AddDbContext<UygulamaDbContext>(...);
builder.Services.AddScoped<IIlerlemeServisi, IlerlemeServisi>();
```

## Bir istek geldiğinde ne olur?
1. `GET /api/konular/5` gelir.
2. ASP.NET Core bir `KonularController` oluşturmak ister ve constructor'ın `IIlerlemeServisi` istediğini görür.
3. Konteyner kayda bakar: `IIlerlemeServisi` → `IlerlemeServisi`.
4. `IlerlemeServisi`'nin constructor'ı da `UygulamaDbContext` istiyor. Konteyner önce onu oluşturur.
5. Zincir tamamlanır, controller hazır.

Buna **bağımlılık ağacı** denir; konteyner ağacı otomatik çözer.

## Service lifetime (servis ömrü)
- **Singleton:** Uygulama boyunca tek nesne. Ayarlar, önbellek için.
- **Scoped:** Her HTTP isteği için bir nesne; istek bitince atılır. **DbContext ve iş servisleri.**
- **Transient:** Her istendiğinde yeni nesne. Hafif, durumsuz yardımcılar için.

Kural: Uzun ömürlü bir servis, kısa ömürlü bir servisi constructor'dan alamaz. Örneğin Singleton bir sınıf Scoped DbContext alırsa uygulama başlarken hata verir.

## Bu kalıbın kazancı
- Controller'a dokunmadan servisi değiştirebilirsin.
- Birim testte sahte (mock) servis verebilirsin.
- Nesne yaşam döngüsünü (oluşturma, kapatma) framework yönetir.
""",

        [("Backend", 4)] = """
## Katmanlar
```
İstek → Controller → Service → DbContext → SQL Server
         (HTTP)     (iş kuralı)  (veri erişimi)
```
- **Controller:** HTTP ile ilgilenir: parametreyi al, servisi çağır, sonucu uygun durum koduna çevir. **İş kuralı içermez.**
- **Service:** İş kuralları burada. Örneğin "bugünün konusu, sıradaki ilk tamamlanmamış konudur".
- **Veri katmanı (DbContext):** Veritabanıyla konuşur.

## Neden ayırıyoruz?
Aynı iş kuralını yarın bir **Windows Service**'ten (sabah hatırlatması) de çağıracağız. Kural controller'ın içinde olsaydı oradan kullanamazdık. Servisteyse her yerden çağrılabilir.

## Entity ve DTO farkı
**Entity** veritabanı tablosunun şeklidir:
```csharp
public class Konu
{
    public int Id { get; set; }
    public int AlanId { get; set; }
    public Alan Alan { get; set; }            // ilişki
    public List<Terim> Terimler { get; set; } // ilişki
    ...
}
```
**DTO** API'nin dışarıya gösterdiği şekildir:
```csharp
public record KonuDetayDto(int Id, string AlanAdi, string Baslik, ..., List<TerimDto> Terimler, ...);
```

Entity'yi doğrudan döndürmüyoruz, çünkü:
1. **Döngüsel referans:** Konu → Alan → Konular → Konu ... JSON'a çevirirken sonsuz döngü oluşur.
2. **Sözleşme:** Tabloya sütun eklemek API'yi kullanan mobil uygulamayı bozmamalı.
3. **Güvenlik:** Kullanıcı tablosunda şifre hash'i var. Entity dönersek o da dışarı sızar.
4. **Performans:** DTO'ya `Select` ile projeksiyon yaparsak SQL'den sadece gereken sütunlar gelir.

## Projedeki klasörler
- `Controllers/`: HTTP katmanı
- `Services/`: İş kuralları
- `Data/`: DbContext, migration'lar, başlangıç verisi
- `Entities/`: Tablo sınıfları
- `Dtos/`: Dışarıya giden veri şekilleri

Büyüyen projelerde bunlar ayrı projelere (Api, Application, Domain, Infrastructure) bölünür. Buna **Clean Architecture** denir; ileride buna da geçebiliriz.
""",

        [("Backend", 5)] = """
## İki ayrı soru
- **Authentication (kimlik doğrulama):** "Sen kimsin?" Kullanıcı adı ve şifreyle kanıtlanır.
- **Authorization (yetkilendirme):** "Bunu yapmaya iznin var mı?" Örneğin sadece admin konu ekleyebilir.

401 → kimliğini bilmiyorum. 403 → kim olduğunu biliyorum ama iznin yok.

## Şifreler asla düz saklanmaz
Veritabanı çalınırsa şifreler de çalınmasın diye **hash** saklanır:
```
"Gizli123" → hash fonksiyonu → "$2a$11$Xk9...q1"
```
Hash tek yönlüdür, geri çevrilemez. Girişte kullanıcının yazdığı şifre tekrar hash'lenir ve kayıtlı hash ile karşılaştırılır. Aynı şifrelerin aynı hash'i üretmemesi için her şifreye rastgele bir **salt** eklenir. ASP.NET Core Identity bunu `PasswordHasher` ile yapar.

## JWT (JSON Web Token)
Giriş başarılıysa sunucu imzalı bir "kimlik kartı" verir:
```
eyJhbGciOiJIUzI1NiJ9 . eyJzdWIiOiI0MiIsIm5hbWUiOiJFbXJlIn0 . SflKxwRJSMeKKF2QT4fw
       başlık         .            içerik (claims)          .        imza
```
İstemci sonraki her istekte bunu başlıkta gönderir:
```
Authorization: Bearer eyJhbGciOi...
```
Sunucu imzayı kendi gizli anahtarıyla doğrular. İmza tutuyorsa içeriğe güvenir; veritabanına sormasına gerek kalmaz.

**Dikkat:** JWT'nin içeriği şifreli değildir, sadece imzalıdır. İçine hassas bilgi konmaz.

## ASP.NET Core'da
```csharp
[Authorize]                       // giriş yapmış olmak gerekir
[HttpPost("{id:int}/tamamla")]
public async Task<IActionResult> Tamamla(int id) { ... }

[Authorize(Roles = "Admin")]      // admin rolü gerekir
[HttpDelete("{id:int}")]
public async Task<IActionResult> Sil(int id) { ... }
```

## Bu projede
Şu an tek kullanıcı var. İlerideki bir adımda kullanıcı kaydı ve girişi ekleyip her kullanıcının kendi ilerlemesini tutacağız. O zaman `Konular.TamamlanmaTarihi` sütunu `KullaniciIlerleme` adlı ayrı bir tabloya taşınacak (çoka çok ilişki).
""",

        [("Backend", 6)] = """
## Middleware pipeline
Her istek, `Program.cs`'te sıraladığımız ara katmanlardan geçer. Cevap da aynı yoldan geri döner:
```
İstek → [HttpsRedirection] → [StaticFiles] → [Routing/Controller] → cevap
                                                    ↓
Cevap ← [HttpsRedirection] ← [StaticFiles]  ←───────┘
```
Bir middleware isteği bir sonrakine iletebilir ya da **kısa devre** yapıp cevabı kendisi döndürebilir. Örneğin `UseStaticFiles` istenen dosyayı bulursa controller'a hiç gitmez.

## Kendi middleware'imiz
```csharp
app.Use(async (context, next) =>
{
    var sayac = Stopwatch.StartNew();
    await next();                                  // sonraki middleware'e geç
    Console.WriteLine($"{context.Request.Path} {sayac.ElapsedMilliseconds} ms");
});
```

## Sıra neden önemli?
```csharp
app.UseAuthentication();   // önce kimsin?
app.UseAuthorization();    // sonra iznin var mı?
```
Ters sırada yazılırsa yetki kontrolü kullanıcının kim olduğunu bilmeden yapılır.

## Loglama
```csharp
public class IlerlemeServisi(..., ILogger<IlerlemeServisi> logger)
{
    logger.LogInformation("Konu tamamlandı. KonuId={KonuId}", konu.Id);
}
```
Log seviyeleri (azdan çoğa önem): `Trace` → `Debug` → `Information` → `Warning` → `Error` → `Critical`.

`appsettings.json` içinde hangi seviyeden itibaren yazılacağı ayarlanır:
```
"LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
```

## Structured logging
`$"KonuId={konu.Id}"` yerine `"KonuId={KonuId}", konu.Id` yazıyoruz. Böylece log sistemleri `KonuId` değerini ayrı bir alan olarak saklar ve "KonuId=5 olan tüm loglar" diye arama yapılabilir.

## Global hata yönetimi
Yakalanmayan her exception kullanıcıya 500 olarak döner. Doğru yaklaşım: Hepsini tek bir middleware'de yakalamak, logu yazmak ve kullanıcıya iç detayları sızdırmadan standart bir hata cevabı (ProblemDetails) dönmek. Bunu ilerideki bir adımda ekleyeceğiz.
""",
    };
}
