# Adım 1: Proje Yapısı, SQL Server ve İlk API

> **Bu adımda ne yaptık?** Boş bir projeden başlayıp SQL Server'a bağlı, katmanlı bir Web API kurduk.
> Uygulama açılınca veritabanını kendisi oluşturuyor, içine yol haritasını (4 alan, 24 konu, 94 terim, 30 görev)
> yüklüyor ve "bugün ne çalışmalıyım?" sorusunu cevaplıyor.

---

## 1. Klasör yapısı: neyi nerede bulursun?

```
AliskanlikApp/
├── dotnet-tools.json            ← Projede kullanılan .NET araçları (dotnet-ef)
├── docs/                        ← 📚 Bu anlatımlar
└── src/YolHaritasi.Api/
    ├── Program.cs               ← ① BAŞLANGIÇ: servis kaydı + istek boru hattı
    ├── appsettings*.json        ← Ayarlar (connection string burada)
    ├── Entities/                ← ② Veritabanı tablolarına karşılık gelen sınıflar
    │   ├── Alan.cs  Konu.cs  Terim.cs  Gorev.cs
    ├── Data/
    │   ├── UygulamaDbContext.cs ← ③ C# ↔ SQL Server köprüsü
    │   ├── BaslangicVerisi.cs   ← Yol haritasının içeriği (seed data)
    │   └── Migrations/          ← ④ Otomatik üretilen veritabanı şema dosyaları
    ├── Dtos/Dtolar.cs           ← ⑤ API'nin dışarıya verdiği veri şekilleri
    ├── Services/
    │   └── IlerlemeServisi.cs   ← ⑥ İş kuralları
    ├── Controllers/             ← ⑦ HTTP isteklerini karşılayan sınıflar
    └── YolHaritasi.Api.http     ← API'yi denemek için hazır istekler
```

**Okuma sırası önerisi:** ② Entities → ③ DbContext → ④ Migration → ① Program.cs → ⑦ Controllers → ⑥ Service → ⑤ DTO.
Yani önce veriyi, sonra veriye nasıl ulaşıldığını oku.

---

## 2. Veri modeli: tablolar ve ilişkiler

```
┌──────────────┐ 1      * ┌──────────────────────┐ 1      * ┌──────────────┐
│   Alanlar    │──────────│       Konular        │──────────│   Terimler   │
├──────────────┤          ├──────────────────────┤          ├──────────────┤
│ Id (PK)      │          │ Id (PK)              │          │ Id (PK)      │
│ Ad (UNIQUE)  │          │ AlanId (FK)          │          │ KonuId (FK)  │
│ Aciklama     │          │ Baslik, Ozet, Sira   │          │ Ad, Tanim    │
│ Sira         │          │ TahminiDakika        │          │ Ornek (NULL) │
└──────────────┘          │ TamamlanmaTarihi(NULL)│         └──────────────┘
                          └──────────┬───────────┘
                                     │ 1
                                     │ *
                              ┌──────┴───────┐
                              │   Gorevler   │
                              ├──────────────┤
                              │ Id (PK)      │
                              │ KonuId (FK)  │
                              │ Aciklama     │
                              │ Sira         │
                              │ Tamamlandi   │
                              └──────────────┘
```
`1 ── *` = **bire çok (one-to-many)**: Bir alanın çok konusu, bir konunun çok terimi ve görevi var.

### C# sınıfı → SQL tablosu nasıl oluştu?
`Entities/Konu.cs` dosyasındaki sınıf:
```csharp
public class Konu
{
    public int Id { get; set; }                     // → PRIMARY KEY, IDENTITY
    public string Baslik { get; set; } = "";        // → nvarchar(200) NOT NULL  (uzunluk DbContext'te)
    public DateTime? TamamlanmaTarihi { get; set; } // → datetime2 NULL  (? = nullable)
    public int AlanId { get; set; }                 // → FOREIGN KEY → Alanlar.Id
    public Alan Alan { get; set; } = null!;         // → sütun DEĞİL, ilişki (navigation property)
    ...
}
```
`dotnet ef migrations add IlkKurulum` komutu bu sınıfları ve DbContext ayarlarını okuyup
`Data/Migrations/..._IlkKurulum.cs` dosyasını **üretti**:
```csharp
migrationBuilder.CreateTable(
    name: "Konular",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
        TamamlanmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
        AlanId = table.Column<int>(type: "int", nullable: false)
        ...
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Konular", x => x.Id);
        table.ForeignKey(name: "FK_Konular_Alanlar_AlanId", column: x => x.AlanId,
            principalTable: "Alanlar", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
    });
```
Uygulama açılınca `db.Database.MigrateAsync()` bu dosyayı SQL'e (`CREATE TABLE ...`) çevirip çalıştırıyor.
Hangi migration'ların uygulandığını SQL Server **`__EFMigrationsHistory`** tablosunda tutuyor.
Böylece aynı migration iki kez uygulanmıyor.

> 💡 **Code-First:** Önce C# sınıflarını yazdık, veritabanını onlardan ürettik.
> Tersi (var olan veritabanından sınıf üretmek) **Database-First** olarak bilinir.

---

## 3. Bir isteğin yolculuğu: `GET /api/bugun`

```
Tarayıcı ──GET /api/bugun──▶ Kestrel (5019 portu)
                               │
                               ▼  Middleware pipeline (Program.cs, B bölümü)
                         UseHttpsRedirection → MapControllers (routing)
                               │
                               ▼  "api/bugun" + GET → BugunController.Getir()
              ┌──────────── BugunController ────────────┐
              │  ilerleme.BugunAsync()                   │  ← DI ile gelen IIlerlemeServisi
              └───────────────┬──────────────────────────┘
                              ▼
              ┌──────────── IlerlemeServisi ────────────┐
              │  İş kuralı: sıradaki tamamlanmamış konu │
              │  db.Konular.Where(...).OrderBy(...)     │  ← LINQ
              └───────────────┬──────────────────────────┘
                              ▼  EF Core LINQ'i SQL'e çevirir
                 SELECT TOP(1) k.Id FROM Konular k
                 INNER JOIN Alanlar a ON k.AlanId = a.Id
                 WHERE k.TamamlanmaTarihi IS NULL
                 ORDER BY a.Sira, k.Sira
                              │
                              ▼
                        🗄️ SQL Server
                              │ satırlar
                              ▼
                 C# nesneleri → BugunDto → JSON → 200 OK → Tarayıcı
```

**Bunu kendi gözünle gör:** `appsettings.Development.json` dosyasında
`"Microsoft.EntityFrameworkCore.Database.Command": "Information"` ayarını açtım. `dotnet run` terminalinde
her istekte EF Core'un SQL Server'a gönderdiği **gerçek SQL'i** göreceksin (`Executed DbCommand` satırları).

---

## 4. Endpoint listesi

| Metot | Adres | Ne yapar | Başarılı cevap |
|---|---|---|---|
| GET | `/api/bugun` | Sıradaki konu, terimleri, görevleri, genel ilerleme % | 200 |
| GET | `/api/alanlar` | 4 alan ve her birinin ilerleme yüzdesi | 200 |
| GET | `/api/alanlar/{id}` | Alanın konuları | 200 / 404 |
| GET | `/api/konular/{id}` | Konu detayı | 200 / 404 |
| POST | `/api/konular/{id}/tamamla` | Konuyu tamamlandı yap | **204** / 404 |
| PATCH | `/api/gorevler/{id}` | Görev durumunu değiştir. Gövde: `{"tamamlandi":true}` | **204** / 404 |
| GET | `/api/terimler?ara=...` | Sözlükte arama | 200 |

### Parametreler nereden geliyor? (Model Binding)
| Kaynak | Örnek | Kodda |
|---|---|---|
| **Route** (adresin içi) | `/api/konular/5` | `[HttpGet("{id:int}")] ... (int id)` |
| **Query string** (`?` sonrası) | `/api/terimler?ara=key` | `[FromQuery] string? ara` |
| **Body** (isteğin gövdesi, JSON) | `{"tamamlandi": true}` | `[FromBody] GorevDurumIstegi istek` |

---

## 5. Bu adımın terimleri

### Mimari
| Terim | Anlamı | Bu projede |
|---|---|---|
| **Katmanlı mimari** | Kodu sorumluluklara göre katmanlara ayırmak | Controller → Service → DbContext |
| **Controller** | HTTP isteğini karşılar, cevaba çevirir. İnce olmalı | `Controllers/` |
| **Service** | İş kurallarının yeri | `IlerlemeServisi` |
| **Entity** | Tabloya karşılık gelen sınıf | `Entities/` |
| **DTO** | Dışarıya giden veri şekli; entity'yi gizler | `Dtos/Dtolar.cs` |
| **Interface** | "Ne yapılır" sözleşmesi | `IIlerlemeServisi` |
| **Dependency Injection** | Bağımlılıkları framework'ün constructor'a vermesi | `IlerlemeServisi(UygulamaDbContext db, ...)` |
| **Service Lifetime** | Singleton (hep aynı) / **Scoped** (istek başına) / Transient (her seferinde yeni) | `AddScoped`, `AddDbContext` |
| **Middleware Pipeline** | İsteğin sırayla geçtiği ara katmanlar | `app.Use...` satırları |
| **OpenAPI** | API'nin makinece okunabilir tarifi | `/openapi/v1.json` |

### Veritabanı
| Terim | Anlamı | Bu projede |
|---|---|---|
| **ORM** | Nesne ↔ tablo eşlemesi | Entity Framework Core |
| **DbContext / DbSet** | Veritabanı oturumu / tablo | `UygulamaDbContext`, `db.Konular` |
| **Migration** | Şema değişikliğinin sürümlü kaydı | `Data/Migrations/` |
| **Primary Key / Identity** | Satırın benzersiz kimliği / otomatik artan | `Id` |
| **Foreign Key** | Başka tabloya bağlantı | `Konular.AlanId` |
| **Unique Index** | Tekrarı engelleyen index | `Alanlar.Ad`, `(AlanId, Sira)` |
| **NULL / Nullable** | Değer yok | `TamamlanmaTarihi`, `DateTime?` |
| **Cascade Delete** | Üst kayıt silinince alttakiler de silinir | Alan silinirse konuları da silinir |
| **Connection String** | Veritabanı adresi ve giriş bilgisi | `appsettings.Development.json` |
| **LocalDB** | Visual Studio ile gelen hafif SQL Server | `(localdb)\MSSQLLocalDB` |
| **Seed Data** | Başlangıç verisi | `BaslangicVerisi.cs` |
| **Idempotent** | Kaç kez çalışırsa çalışsın sonuç aynı | Seed sadece tablo boşsa ekler |
| **Change Tracking** | EF Core'un nesne değişikliklerini izlemesi | `KonuyuTamamlaAsync` |
| **AsNoTracking** | Sadece okumada takibi kapatıp hızlanmak | Okuma sorguları |
| **Projection** | `Select` ile sadece gereken sütunları çekmek | `.Select(k => new KonuDetayDto(...))` |
| **IQueryable** | Henüz çalışmamış, adım adım kurulan sorgu | `TerimlerController.Ara` |
| **SQL Injection** | Kullanıcı girdisiyle SQL'i manipüle etme saldırısı. Parametreli sorgu bunu önler | EF Core otomatik parametreler |
| **Transaction** | Ya hepsi ya hiçbiri | `SaveChangesAsync` |

### HTTP
| Terim | Anlamı |
|---|---|
| **204 No Content** | Başarılı, ama cevap gövdesi yok |
| **Safe method** | Veriyi değiştirmeyen metot (GET). Bu yüzden "tamamla" işlemi POST |
| **PATCH vs PUT** | PATCH kısmi günceller, PUT kaynağın tamamını değiştirir |
| **UTC** | Saat diliminden bağımsız evrensel zaman. Sunucularda zaman hep UTC saklanır |
| **LTS** | Long Term Support. .NET 10 LTS, 3 yıl destek alır. Sunucularda tercih edilir |

---

## 6. 🖥️ Bilgisayarında çalıştır

### Bir kerelik kurulum
1. **Visual Studio 2026 Community** kur. Kurulumda **"ASP.NET and web development"** iş yükünü seç.
   Bu iş yükü .NET SDK'yı ve **SQL Server LocalDB**'yi de kurar.
2. **SQL Server Management Studio (SSMS)** kur. Veritabanını görsel olarak incelemek için kullanacağız.
3. Kontrol et (PowerShell):
   ```powershell
   dotnet --version        # 10.x
   sqllocaldb info         # MSSQLLocalDB yazmalı
   ```

### Güncel kodu al ve çalıştır
```powershell
cd ~\Projeler\AliskanlikApp
git pull
cd src\YolHaritasi.Api
dotnet run
```
İlk çalıştırmada `YolHaritasiDb` veritabanı **otomatik oluşturulur ve doldurulur**.

Tarayıcıda **http://localhost:5019** adresini aç. **Scalar** arayüzü açılır: tüm endpoint'leri görüp
"Test Request" ile deneyebilirsin. Ya da Visual Studio'da `YolHaritasi.Api.http` dosyasını açıp
isteklerin yanındaki **Send Request**'e tıkla.

### SSMS ile veritabanına bak
1. SSMS'i aç. Server name: `(localdb)\MSSQLLocalDB`, Authentication: **Windows Authentication**
2. `Databases → YolHaritasiDb → Tables` altında 4 tablomuzu ve `__EFMigrationsHistory` tablosunu gör
3. `Konular` tablosuna sağ tıkla: **Design** ile sütun tiplerini, **Keys / Indexes** klasörleriyle anahtarları incele
4. **New Query** açıp şunları çalıştır:
   ```sql
   -- Her alanın konu sayısı ve tamamlanan konu sayısı
   SELECT a.Ad,
          COUNT(*) AS ToplamKonu,
          SUM(CASE WHEN k.TamamlanmaTarihi IS NOT NULL THEN 1 ELSE 0 END) AS Tamamlanan
   FROM Konular k
   JOIN Alanlar a ON a.Id = k.AlanId
   GROUP BY a.Ad;

   -- Bugünün konusu (API'nin yaptığı sorgunun aynısı)
   SELECT TOP 1 a.Ad, k.Baslik
   FROM Konular k JOIN Alanlar a ON a.Id = k.AlanId
   WHERE k.TamamlanmaTarihi IS NULL
   ORDER BY a.Sira, k.Sira;
   ```
5. Scalar'dan `POST /api/konular/1/tamamla` gönder, sonra SSMS'te tekrar `SELECT * FROM Konular` çalıştır.
   `TamamlanmaTarihi` sütununun dolduğunu gör. **API → EF Core → SQL Server zincirini uçtan uca izlemiş oldun.**

### Sık karşılaşılan durumlar
| Görüntü | Sebep / Çözüm |
|---|---|
| `Failed to determine the https port for redirect` uyarısı | `http` profilinde çalışıyorsun ve https adresi yok. Zararsız. `dotnet run --launch-profile https` ile kaybolur |
| `A network-related or instance-specific error ... (localdb)` | LocalDB kurulu değil veya çalışmıyor. `sqllocaldb start MSSQLLocalDB` dene; olmazsa VS Installer'dan iş yükünü kontrol et |
| İlerlemeyi sıfırlamak istiyorum | SSMS'te: `UPDATE Konular SET TamamlanmaTarihi = NULL; UPDATE Gorevler SET Tamamlandi = 0;` |
| Veritabanını tamamen silip baştan kurmak | `dotnet ef database drop` (proje klasöründe; önce bir kez `dotnet tool restore`) → sonra `dotnet run` |

---

## 7. Merak edersen kendin dene (isteğe bağlı)
- `BaslangicVerisi.cs` dosyasına yeni bir terim ekle. Veritabanı doluyken eklenmeyeceğini gör (idempotent!). DB'yi silip tekrar çalıştırınca gelir.
- Terminaldeki SQL loglarında `GET /api/alanlar` isteğinin ürettiği SQL'i bul ve SSMS'te aynen çalıştır.

---
➡️ **Sonraki adım (2):** Konulara **not alma** ve **çalışma oturumu** kaydı (ne kadar süre çalıştın).
Gerçek CRUD, veri doğrulama (validation) ve istatistik sorguları (GROUP BY) gelecek.
