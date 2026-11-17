# Adım 2: Ders Notları, Kaynaklar, Projeler ve Veritabanı Ekranı

> **Bu adımda ne yaptık?**
> - Yeni bir alan: **C# Temelleri** (10 konu). Backend'den önce dilin kendisi.
> - C# ve Backend konularına **Türkçe ders notları** (kod örnekli), tüm alanlara **W3Schools / Microsoft Learn kaynak linkleri**.
> - **🧪 Projeler:** Her alan için adım adım ilerleyen pratik projeler.
> - **🗄️ Veritabanı** sekmesi: bağlantı bilgisi, tablolar ve sütunlar, **SQL pratik alanı**.
> - Var olan veritabanını **silmeden** güncelleyen ikinci migration ve akıllı başlangıç verisi.

---

## 1. Veri modelindeki değişiklik

```
Alanlar ──1:*── Konular ──1:*── Terimler
   │               ├──1:*── Gorevler
   │               └──1:*── Kaynaklar        ← YENİ
   │               (+ DersNotu sütunu)        ← YENİ
   └──1:*── Projeler ──1:*── ProjeAdimlari    ← YENİ
```

Yeni dosyalar: `Entities/Kaynak.cs`, `Entities/Proje.cs`. `Konu.cs` dosyasına `DersNotu` eklendi.

## 2. İkinci migration: veritabanını silmeden güncellemek

Entity'leri değiştirdikten sonra şu komutu çalıştırdım:
```
dotnet ef migrations add DersNotlariKaynaklarProjeler
```
EF Core, `Data/Migrations/UygulamaDbContextModelSnapshot.cs` dosyasındaki "son bilinen model" ile şimdiki modeli **karşılaştırdı** ve sadece farkı üretti:
```csharp
migrationBuilder.AddColumn<string>(name: "DersNotu", table: "Konular", type: "nvarchar(max)", nullable: true);
migrationBuilder.CreateTable(name: "Kaynaklar", ...);
migrationBuilder.CreateTable(name: "Projeler", ...);
migrationBuilder.CreateTable(name: "ProjeAdimlari", ...);
migrationBuilder.CreateIndex(name: "IX_Kaynaklar_KonuId", ...);   // foreign key'lere otomatik index
```
Uygulama açılınca `MigrateAsync()` `__EFMigrationsHistory` tablosuna bakar. `IlkKurulum` zaten uygulanmış, `DersNotlariKaynaklarProjeler` uygulanmamış. Sadece yenisini çalıştırır. **Senin verine ve ilerlemene dokunulmaz.**

> 💡 `DersNotu` sütunu neden `nullable: true`? Tabloda zaten satırlar var. NOT NULL bir sütun eklenseydi
> mevcut satırlara ne yazılacağını söylememiz gerekirdi. Var olan bir tabloya sütun eklerken bu hep düşünülmesi gereken bir konudur.

## 3. Başlangıç verisi artık "eksik olanı" ekliyor

Adım 1'de seed sadece tablo **boşsa** çalışıyordu. Bu yüzden yeni içerik eklemek için veritabanını silmek gerekiyordu.
Artık `BaslangicVerisi.EkleAsync` her açılışta şunları yapıyor:
1. Veritabanında **olmayan alanları** ekler (C# Temelleri böyle geldi).
2. Ders notu **boş** olan konulara ders notunu yazar (`UPDATE`).
3. **Hiç kaynağı olmayan** konulara linkleri ekler (`INSERT`).
4. **Hiç projesi olmayan** alanlara projeleri ekler.

İçerik büyüdüğü için sınıf **partial** yapıldı ve dosyalara bölündü:
```
Data/BaslangicVerisi.cs              ← EkleAsync + Backend, SQL, Sunucu, Yapay Zeka alanları
Data/BaslangicVerisi.CSharp.cs       ← C# Temelleri alanı
Data/BaslangicVerisi.DersNotlari.cs  ← Türkçe ders notları ("""raw string literal""")
Data/BaslangicVerisi.Kaynaklar.cs    ← W3Schools / Microsoft Learn linkleri
Data/BaslangicVerisi.Projeler.cs     ← Proje görevleri
```

## 4. Yeni endpoint'ler

| Metot | Adres | Ne yapar |
|---|---|---|
| GET | `/api/projeler` | Projeler, adımları, ilerleme |
| PATCH | `/api/projeler/adimlar/{id}` | Adımı işaretle. Gövde: `{"tamamlandi":true}` |
| GET | `/api/veritabani/durum` | Bağlantı, sürüm, migration'lar, tablolar, sütunlar, kayıt sayıları |
| POST | `/api/veritabani/sorgu` | SELECT sorgusu çalıştır. Gövde: `{"sql":"SELECT ..."}` |

`GET /api/konular/{id}` ve `GET /api/bugun` cevaplarına `dersNotu` ve `kaynaklar` eklendi.

## 5. Okunması gereken yeni kodlar

### `ProjelerController.cs`: iki farklı güncelleme yöntemi
`GorevlerController` görevi önce **okuyor**, sonra değiştirip **kaydediyor** (2 SQL komutu: SELECT + UPDATE).
`ProjelerController` ise **tek komut** gönderiyor:
```csharp
await db.ProjeAdimlari
    .Where(a => a.Id == id)
    .ExecuteUpdateAsync(s => s.SetProperty(a => a.Tamamlandi, istek.Tamamlandi));
// → UPDATE ProjeAdimlari SET Tamamlandi = @p WHERE Id = @id
```
Terminaldeki SQL loglarında ikisini karşılaştır.

### `VeritabaniServisi.cs`: ADO.NET ile ham SQL
EF Core'un altında yatan katmanı kullanıyor:
```csharp
var baglanti = db.Database.GetDbConnection();      // DbConnection
await using var komut = baglanti.CreateCommand();  // DbCommand
komut.CommandText = "SELECT ...";
await using var okuyucu = await komut.ExecuteReaderAsync();   // DbDataReader
while (await okuyucu.ReadAsync()) { ... okuyucu.GetString(0) ... }
```
Tablo ve sütun bilgileri SQL Server'ın **sistem kataloğundan** (`sys.tables`, `sys.columns`, `sys.foreign_key_columns`) okunuyor.

### SQL pratik alanının güvenliği: derinlemesine savunma
Kullanıcının veritabanında SQL çalıştırması tehlikelidir. Bu yüzden birden çok katman var:
1. **Girdi kontrolü:** `SELECT` veya `WITH` ile başlamalı. `;`, `DELETE`, `DROP`, `EXEC`... yasak (Regex).
2. **Transaction + ROLLBACK:** Sorgu bir transaction içinde çalışıyor ve sonunda **her durumda geri alınıyor**.
3. **Sınırlar:** 5 saniye zaman aşımı, en fazla 200 satır.
4. **Ortam:** Sadece `Development` ortamında açık. Canlı sunucuda 403 döner.

Tek bir kontrole güvenmemek, güvenlikte temel prensiptir (**defense in depth**).

## 6. Arayüzdeki yenilikler
- **Bugün / konu kartı:** 📘 Ders Notu (açılır kapanır), 🔗 Kaynaklar, 📖 Terimler, 🛠️ Görevler
- **🧪 Projeler:** Adımları işaretle, ilerleme çubuğu güncellenir. 💡 ipuçları ilgili konuya yönlendirir.
- **🗄️ Veritabanı:** Bağlantı bilgisi, tablolara tıklayıp sütunları gör (🔑 primary key, 🔗 foreign key), hazır örnek sorgular ve SQL pratik alanı.

Ders notları veritabanında **Markdown** olarak duruyor; `uygulama.js` içindeki `markdown()` fonksiyonu bunu HTML'e çeviriyor.
Çevirmeden önce tüm metni `guvenli()` ile kaçırıyor, böylece veritabanından gelen metin asla kod olarak çalışamıyor (XSS koruması).

## 7. Bu adımın terimleri

| Terim | Anlamı | Bu projede |
|---|---|---|
| **Model Snapshot** | EF Core'un son migration'daki modeli hatırladığı dosya. Yeni migration bununla karşılaştırılarak üretilir | `UygulamaDbContextModelSnapshot.cs` |
| **Schema Migration** | Var olan veritabanını veri kaybetmeden yeni yapıya taşımak | `DersNotlariKaynaklarProjeler` |
| **nvarchar(max)** | Uzunluk sınırı olmayan metin sütunu (2 GB'a kadar) | `Konular.DersNotu` |
| **Include (Eager Loading)** | İlişkili tabloyu aynı sorguda getirmek (JOIN) | `BaslangicVerisi.EkleAsync` |
| **ExecuteUpdate** | Nesneyi belleğe çekmeden doğrudan UPDATE göndermek | `ProjelerController` |
| **ADO.NET** | .NET'in en alt seviye veritabanı erişim katmanı | `VeritabaniServisi` |
| **DbConnection / DbCommand / DbDataReader** | Bağlantı / SQL komutu / satır satır okuyucu | `VeritabaniServisi` |
| **ExecuteScalar** | Sorgunun ilk satırının ilk sütununu döndürür | `SELECT @@VERSION` |
| **System Catalog** | SQL Server'ın kendi yapısı hakkındaki tablolar | `sys.tables`, `sys.columns` |
| **Rollback** | Transaction'daki değişiklikleri geri almak | SQL pratik alanı |
| **Defense in Depth** | Güvenliği birden çok bağımsız katmanla sağlamak | SQL pratik alanı |
| **partial class** | Bir sınıfı birden çok dosyaya bölmek | `BaslangicVerisi.*.cs` |
| **Raw String Literal** | `"""` ile yazılan, kaçış karakteri gerektirmeyen çok satırlı metin | Ders notları |
| **GeneratedRegex** | Regex'i derleme zamanında üreten attribute | `VeritabaniServisi.YasakliKelime` |
| **Custom Exception** | Kendi hata tipimiz | `SorguHatasi` → 400 |

## 8. 🖥️ Bilgisayarında

Proje klasöründe:
```powershell
git pull
cd src\YolHaritasi.Api
dotnet run
```
Bu sefer **veritabanını silmene gerek yok**. Uygulama açılırken yeni migration'ı uygular ve eksik içeriği ekler. İlerlemen korunur.

Terminalde şu satırları gör:
```
Applying migration '2026..._DersNotlariKaynaklarProjeler'.
```
Sonra SSMS'te `Kaynaklar`, `Projeler` ve `ProjeAdimlari` tablolarına bak.

> ⚠️ **W3Schools linkleri hakkında:** Linkler W3Schools'un bilinen ders adreslerinden oluşturuldu. Ancak geliştirme ortamım
> W3Schools'a erişemediği için tek tek açılıp kontrol edilemedi. Açılmayan bir link görürsen bana yaz, düzelteyim.
> W3Schools'un ders metinleri telif hakkıyla korunduğu için uygulamaya kopyalanmadı. Uygulamadaki ders notları
> bu proje için ayrıca yazıldı, detay için linkler verildi.

---
➡️ **Sonraki adım (3):** Kullanıcı kaydı ve girişi (JWT). Her kullanıcının kendi ilerlemesi olacak.
Mevcut `TamamlanmaTarihi` ve `Tamamlandi` sütunları `KullaniciIlerleme` tablosuna taşınacak. Bu da gerçek bir **veri taşıma (data migration)** örneği olacak.
