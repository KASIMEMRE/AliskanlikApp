using YolHaritasi.Api.Entities;

namespace YolHaritasi.Api.Data;

// "partial" = Bu sınıf birden fazla dosyaya bölünmüş. Derleyici hepsini tek sınıf olarak birleştirir.
// İçerik çok büyük olduğu için her alanı ayrı dosyaya koyduk.
public static partial class BaslangicVerisi
{
    // =====================================================================
    //  0) C# TEMELLERİ: Backend'e geçmeden önce dilin kendisi
    //  Sira = 0 → listede ve "Bugün" ekranında en başa gelir.
    // =====================================================================
    private static Alan CSharpTemelleri() => new()
    {
        Sira = 0,
        Ad = "C# Temelleri",
        Aciklama = "Backend'e geçmeden önce C# dilinin temelleri: değişkenlerden nesne yönelimli programlamaya.",
        Konular =
        [
            K(1, "C#'a Giriş ve İlk Program", "C# nedir, .NET ile ilişkisi ve ilk konsol uygulaması.", 45,
            [
                (".NET", "Microsoft'un uygulama platformu: derleyici, çalışma ortamı (runtime) ve hazır kütüphaneler.", null),
                ("SDK", "Geliştirme kiti. Kod yazmak ve derlemek için gerekir.", "dotnet --version"),
                ("Compiler", "Derleyici: C# kodunu bilgisayarın çalıştırabileceği ara koda (IL) çeviren program.", null),
                ("Console Application", "Penceresi olmayan, komut satırında çalışan uygulama.", "dotnet new console -n Merhaba"),
                ("Namespace", "Sınıfları gruplayan isim alanı. Aynı isimli sınıfların karışmasını önler.", "namespace YolHaritasi.Api.Entities;"),
            ],
            [
                "dotnet new console -n Merhaba ile bir proje oluştur ve dotnet run ile çalıştır.",
                "Console.WriteLine ile kendi adını ve bugünün tarihini (DateTime.Now) yazdır.",
            ]),

            K(2, "Değişkenler ve Veri Tipleri", "Veriyi bellekte saklamak: int, string, bool, double ve tip dönüşümleri.", 60,
            [
                ("Variable", "Değişken: Bir değeri saklayan, adı olan bellek alanı.", "int yas = 25;"),
                ("Data Type", "Değişkenin ne tür veri tuttuğu: int (tam sayı), double (ondalıklı), bool (doğru/yanlış), string (metin), char (tek karakter).", null),
                ("var", "Tipi derleyicinin sağ taraftan kendisinin anlaması. Tip yine sabittir, sadece yazmayız.", "var sehir = \"İstanbul\";  // string"),
                ("Type Casting", "Bir tipi başka bir tipe dönüştürme.", "int sayi = int.Parse(\"42\");"),
                ("Null", "'Değer yok' demek. string? gibi soru işaretli tipler null olabilir.", "string? not = null;"),
            ],
            [
                "Kullanıcıdan Console.ReadLine ile yaşını al, int.Parse ile sayıya çevir ve 10 yıl sonraki yaşını yazdır.",
                "Sayı yerine harf girildiğinde ne olduğunu gözlemle (bu hatayı 9. konuda çözeceğiz).",
            ]),

            K(3, "Operatörler, String ve Math", "Hesaplama, karşılaştırma ve metinlerle çalışmak.", 60,
            [
                ("Operator", "İşlem yapan sembol: + - * / % (aritmetik), == != > < (karşılaştırma), && || ! (mantıksal).", null),
                ("String Interpolation", "Metnin içine değişken gömme. Başına $ konur.", "$\"Merhaba {ad}, {yas} yaşındasın\""),
                ("Immutable", "Değiştirilemez. string'ler immutable'dır; değiştirdiğini sandığında aslında yeni bir string oluşur.", null),
                ("Method Chaining", "Metotları art arda çağırmak.", "ad.Trim().ToUpper()"),
            ],
            [
                "Bir metnin uzunluğunu, büyük harfli halini ve içinde 'C#' geçip geçmediğini yazdıran kod yaz.",
            ]),

            K(4, "Koşullar ve Döngüler", "Karar vermek (if, switch) ve tekrar etmek (for, while, foreach).", 75,
            [
                ("Condition", "Koşul: true/false sonuç veren ifade. if, else if, else ile kullanılır.", "if (puan >= 50) { ... }"),
                ("Switch Expression", "Bir değere göre farklı sonuç üretmenin kısa yolu.", "var ad = gun switch { 1 => \"Pazartesi\", _ => \"?\" };"),
                ("Loop", "Döngü: Bir kod bloğunu tekrar tekrar çalıştırma. for, while, do-while, foreach.", null),
                ("break / continue", "break döngüden çıkar, continue o turu atlayıp sonrakine geçer.", null),
            ],
            [
                "1'den 100'e kadar sayılarda 3'e bölünenlere 'Fizz', 5'e bölünenlere 'Buzz', ikisine birden bölünenlere 'FizzBuzz' yazdır.",
            ]),

            K(5, "Diziler ve Koleksiyonlar", "Birden fazla değeri bir arada tutmak: Array, List, Dictionary.", 75,
            [
                ("Array", "Sabit uzunlukta, aynı tipte elemanlar dizisi. İndeks 0'dan başlar.", "int[] notlar = [70, 85, 90];"),
                ("List<T>", "Boyutu değişebilen liste. Backend'de en çok kullanılan koleksiyon.", "var konular = new List<string>();"),
                ("Dictionary<TKey, TValue>", "Anahtar-değer çiftleri. Anahtarla çok hızlı arama yapılır.", "var stok = new Dictionary<string, int>();"),
                ("Generic", "<T> ile tipin sonradan belirlendiği yapı. List<int>, List<string> aynı sınıftır.", null),
                ("Index", "Elemanın sıra numarası (0'dan başlar).", "notlar[0]  // 70"),
            ],
            [
                "Bir List<string> içine 5 konu ekle, foreach ile yazdır, sonra birini Remove ile sil.",
                "Dictionary ile kelime → anlam sözlüğü yap ve bir kelimeyi ara (TryGetValue).",
            ]),

            K(6, "Metotlar", "Kodu tekrar kullanılabilir parçalara bölmek.", 60,
            [
                ("Method", "Metot: Adı olan, çağrılabilen kod bloğu. Parametre alır, değer döndürebilir.", "int Topla(int a, int b) => a + b;"),
                ("Parameter / Argument", "Parametre metodun tanımındaki değişken, argüman çağırırken verilen değerdir.", null),
                ("Return Type", "Metodun döndürdüğü değerin tipi. Bir şey döndürmüyorsa void.", null),
                ("Overloading", "Aynı isimde, farklı parametreli birden fazla metot.", null),
                ("Expression-bodied Member", "Tek satırlık metodun => ile kısa yazımı.", "public override string ToString() => Ad;"),
            ],
            [
                "Bir not listesini alıp ortalamasını döndüren double Ortalama(List<int> notlar) metodunu yaz.",
            ]),

            K(7, "OOP: Sınıf ve Nesne", "Nesne yönelimli programlama: class, nesne, constructor, property.", 90,
            [
                ("OOP", "Object-Oriented Programming: Programı birbiriyle konuşan nesneler olarak tasarlama yaklaşımı.", null),
                ("Class", "Sınıf: Nesnenin kalıbı (planı). Hangi verileri ve davranışları olacağını tanımlar.", "public class Konu { ... }"),
                ("Object / Instance", "Nesne: Sınıftan new ile üretilen somut örnek.", "var k = new Konu();"),
                ("Constructor", "Yapıcı metot: Nesne oluşturulurken çalışır, başlangıç değerlerini verir.", null),
                ("Property", "Sınıfın verisine kontrollü erişim sağlayan get/set üyesi.", "public string Ad { get; set; }"),
                ("Access Modifier", "Erişim belirleyici: public (herkes), private (sadece sınıfın içi), protected, internal.", null),
                ("Encapsulation", "Kapsülleme: Veriyi gizleyip sadece kontrollü yollarla değiştirilmesine izin vermek.", null),
            ],
            [
                "Bir BankaHesabi sınıfı yaz: Bakiye dışarıdan doğrudan değiştirilemesin (private set), ParaYatir ve ParaCek metotları olsun; bakiyeden fazla para çekilemesin.",
            ]),

            K(8, "OOP: Kalıtım, Interface ve Polimorfizm", "Sınıflar arası ilişki ve sözleşmeler: backend mimarisinin temeli.", 90,
            [
                ("Inheritance", "Kalıtım: Bir sınıfın başka bir sınıftan özelliklerini devralması.", "class Ogrenci : Kisi"),
                ("Interface", "Bir sınıfın ne yapabileceğini tanımlayan sözleşme. Nasıl yapacağını içermez.", "public interface IIlerlemeServisi"),
                ("Polymorphism", "Çok biçimlilik: Aynı çağrının nesnenin gerçek tipine göre farklı davranması.", null),
                ("Abstract Class", "Soyut sınıf: Doğrudan new'lenemeyen, alt sınıflar için ortak temel.", null),
                ("Enum", "Sabit seçeneklerin isimli listesi.", "enum Zorluk { Kolay, Orta, Zor }"),
                ("record", "Değer eşitliğine sahip, kısa yazılan, genelde değiştirilemez veri tipi. DTO'lar için ideal.", "public record TerimDto(int Id, string Ad);"),
            ],
            [
                "IBildirimGonderici interface'i ve onu uygulayan EpostaGonderici, SmsGonderici sınıflarını yaz; bir listeye koyup döngüyle hepsine Gonder() çağır.",
                "Bu projedeki IIlerlemeServisi ve IlerlemeServisi dosyalarını aç: interface ile sınıfın ilişkisini bul.",
            ]),

            K(9, "Hata Yönetimi ve Dosyalar", "Programın çökmesini önlemek ve dosyaya yazıp okumak.", 60,
            [
                ("Exception", "Çalışma anında oluşan hata nesnesi. Yakalanmazsa program durur.", "FormatException, NullReferenceException"),
                ("try / catch / finally", "Hatayı yakalama bloğu. finally her durumda çalışır.", null),
                ("throw", "Bilerek hata fırlatmak.", "throw new ArgumentException(\"Tutar negatif olamaz\");"),
                ("TryParse", "Hata fırlatmadan dönüştürmeyi deneyen metot. Başarılıysa true döner.", "if (int.TryParse(girdi, out var sayi)) { ... }"),
                ("using", "İşi bitince kaynağı (dosya, bağlantı) otomatik kapatan yapı.", "using var dosya = File.OpenRead(\"a.txt\");"),
            ],
            [
                "2. konudaki yaş programını int.TryParse ile hataya dayanıklı hale getir.",
                "Konu listesini File.WriteAllLines ile dosyaya yaz, File.ReadAllLines ile geri oku.",
            ]),

            K(10, "LINQ ve async/await", "Backend kodunun her satırında göreceğin iki modern C# özelliği.", 90,
            [
                ("LINQ", "Koleksiyonlar ve veritabanı üzerinde sorgu yazma dili: Where, Select, OrderBy, First, Count...", "konular.Where(k => k.Sure > 60).Select(k => k.Ad)"),
                ("Lambda Expression", "İsimsiz kısa fonksiyon. => 'gider' diye okunur.", "k => k.Sure > 60"),
                ("async / await", "Bekleme gerektiren işlemlerde (veritabanı, ağ, dosya) iş parçacığını bloklamadan beklemek.", "var liste = await db.Konular.ToListAsync();"),
                ("Task", "Henüz bitmemiş olabilen bir işi temsil eden nesne. async metotlar Task döndürür.", "public async Task<BugunDto> BugunAsync()"),
                ("Deferred Execution", "Ertelenmiş çalıştırma: LINQ sorgusu tanımlandığında değil, sonuç istendiğinde (ToList, foreach) çalışır.", null),
            ],
            [
                "Bir öğrenci listesinde notu 50'nin üzerindekileri isme göre sıralayıp sadece adlarını LINQ ile al.",
                "Bu projedeki IlerlemeServisi.cs dosyasında her await kullanımını bul ve neyi beklediğini söyle.",
            ]),
        ],
    };
}
