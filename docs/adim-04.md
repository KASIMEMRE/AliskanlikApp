# Adım 4: Kullanıcı Kaydı, Giriş (JWT) ve Kişisel İlerleme

> **Bu adımda ne yaptık?**
> - Uygulamaya **kayıt ol / giriş yap** ekranı geldi. Şifreler **hash**'lenerek saklanıyor.
> - Giriş yapınca sunucu bir **JWT** (token) veriyor; tarayıcı her istekte bunu gönderiyor.
> - İlerleme artık **kişiye özel**: Her kullanıcının tamamladığı konular, görevler ve proje adımları ayrı tutuluyor.
> - Eski (tek kişilik) ilerleme **kaybolmadan** ilk kayıt olan kullanıcıya taşındı.
> - Varsayılan olarak **her endpoint giriş istiyor** (secure by default); SQL pratik alanını sadece **Admin** kullanabiliyor.

---

## 1. Büyük resim: Bir giriş nasıl çalışır?

```
 Tarayıcı                                   Sunucu (YolHaritasi.Api)                 SQL Server
    │  POST /api/hesap/giris                     │                                        │
    │  {"email":"...","sifre":"..."}  ─────────▶ │ SELECT * FROM Kullanicilar WHERE Email │
    │                                            │ ─────────────────────────────────────▶ │
    │                                            │ PasswordHasher: şifre ↔ hash doğru mu? │
    │                                            │ TokenServisi: JWT üret ve imzala       │
    │  ◀───────── 200 {"token":"eyJ..."}         │                                        │
    │  (token localStorage'a kaydedilir)         │                                        │
    │                                            │                                        │
    │  GET /api/bugun                            │                                        │
    │  Authorization: Bearer eyJ...  ──────────▶ │ UseAuthentication: imza doğru mu?      │
    │                                            │   süresi dolmuş mu? → User = kullanıcı │
    │                                            │ UseAuthorization: giriş yapmış mı?     │
    │                                            │ BugunController: User.KullaniciId()    │
    │  ◀───────── 200 { bugünün konusu }         │                                        │
```
**Sunucu oturum bilgisi saklamıyor.** Kimin istek attığını her seferinde token'dan okuyor. Buna **stateless (durumsuz)** kimlik doğrulama denir ve sunucuyu çoğaltmayı (birden fazla sunucu) kolaylaştırır.

## 2. Veri modeli: Çoka çok ilişki

Önce "tamamlandı" bilgisi konunun kendisindeydi (`Konular.TamamlanmaTarihi`), yani tek kişilikti. Şimdi:
```
Kullanicilar ──1:*── KonuIlerlemeleri ──*:1── Konular
      │                (KullaniciId, KonuId, TamamlanmaTarihi)
      ├──1:*── GorevIlerlemeleri ──*:1── Gorevler
      └──1:*── ProjeAdimIlerlemeleri ──*:1── ProjeAdimlari
```
- `KonuIlerlemeleri` bir **bağlantı tablosu** (junction table): "Hangi kullanıcı hangi konuyu ne zaman tamamladı?"
- Primary key iki sütundan oluşuyor: `(KullaniciId, KonuId)` → **composite key**. Aynı kişi aynı konuyu iki kez "tamamlayamaz".
- Dosyalar: `Entities/Kullanici.cs`, `Entities/Ilerleme.cs`. Ayarlar: `UygulamaDbContext.cs` → `HasKey(x => new { x.KullaniciId, x.KonuId })`

## 3. Veri kaybetmeden taşıma (data migration) — bu adımın en önemli dersi

EF Core migration'ı ürettiğinde şunu yazdı:
```csharp
migrationBuilder.DropColumn(name: "TamamlanmaTarihi", table: "Konular");
```
Bu satır sütunu **içindeki veriyle birlikte siler**. EF Core şemayı bilir, verinin anlamını bilmez. Bu yüzden migration'ı **elle düzenledim**
(`Data/Migrations/..._KullanicilarVeKisiselIlerleme.cs`), silmeden önce veriyi geçici bir tabloya kopyaladım:
```csharp
migrationBuilder.Sql("""
    CREATE TABLE EskiIlerleme (Tur nvarchar(20), KayitId int, Tarih datetime2);
    INSERT INTO EskiIlerleme SELECT 'Konu', Id, TamamlanmaTarihi FROM Konular WHERE TamamlanmaTarihi IS NOT NULL;
    ...
    """);
migrationBuilder.DropColumn(...);   // artık güvenli
```
Henüz kullanıcı olmadığı için ilerlemenin sahibi yok. İlk kayıt olan kişi uygulamanın sahibi olduğundan
`HesapServisi.EskiIlerlemeyiTasiAsync` ilerlemeyi ona aktarıp geçici tabloyu siliyor (ham SQL, `ExecuteSqlAsync`).

> 💡 **Kural:** Bir sütunu silen veya tipini değiştiren her migration'ı uygulamadan önce **oku**. Canlı ortamda da önce yedek al.

## 4. Şifre güvenliği (`Services/HesapServisi.cs`)
```csharp
kullanici.SifreHash = _hasher.HashPassword(kullanici, istek.Sifre);
// → "AQAAAAIAAYagAAAAE..." (PBKDF2 + rastgele salt + 100.000 tekrar)

_hasher.VerifyHashedPassword(kullanici, kullanici.SifreHash, girilenSifre)
```
- Kendi hash algoritmanı yazma; `PasswordHasher` gibi denenmiş kütüphaneler kullan.
- **Aynı hata mesajı:** "E-posta yok" ile "şifre yanlış" için aynı mesaj dönüyor. Farklı olsaydı saldırgan hangi e-postaların kayıtlı olduğunu öğrenebilirdi (**user enumeration**).
- E-posta küçük harfe çevrilip saklanıyor: `Emre@Ornek.com` ile `emre@ornek.com` aynı hesap.

## 5. JWT (`Auth/TokenServisi.cs`)
Bir token'ı **jwt.io** sitesine yapıştırırsan (sadece geliştirme token'ıyla dene) içeriğini görürsün:
```json
{ "sub": "1", "name": "Emre", "role": "Admin", "email": "emre@ornek.com",
  "iss": "YolHaritasi.Api", "aud": "YolHaritasi.Istemci", "exp": 1795003426 }
```
- İçerik **şifreli değil**, sadece **imzalı**. Bu yüzden içine şifre gibi gizli bilgi konmaz.
- İmza `Jwt:Anahtar` ile atılıyor. Bu anahtarı bilen herkes sahte token üretebilir. Bu yüzden:
  - Geliştirmede `appsettings.Development.json` içinde (bu dosyadaki anahtar sadece senin bilgisayarın için)
  - Sunucuda **ortam değişkeni** (`Jwt__Anahtar`) veya gizli anahtar kasası. `appsettings.json`'da boş bırakıldı.
- Anahtar eksikse uygulama **açılmıyor** (`Program.cs` → fail fast). Hatayı canlıda ilk istekte değil, başlangıçta görmek daha iyi.

## 6. Yetkilendirme (`Program.cs`)
```csharp
options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
```
**Fallback policy:** Üzerinde `[Authorize]` veya `[AllowAnonymous]` olmayan **her endpoint giriş ister**.
Yeni bir controller yazıp korumayı unutsan bile açık kalmaz (**secure by default**). Açık olması gerekenler:
| Endpoint | Neden açık? |
|---|---|
| `POST /api/hesap/kayit`, `/giris` | `[AllowAnonymous]`: giriş yapmadan kayıt olunabilmeli |
| `GET /api/terimler` | `[AllowAnonymous]`: sözlük herkese açık |
| `/scalar`, `/openapi/v1.json` | `.AllowAnonymous()`: dokümantasyon |
| `index.html`, `uygulama.js` | Statik dosyalar authorization'dan önce sunuluyor |

**Rol tabanlı yetki:** `VeritabaniController` → `[Authorize(Roles = Roller.Admin)]`. Artık `Kullanicilar` tablosunda şifre hash'leri var;
herkes SQL çalıştırabilseydi başkalarının hash'lerini görebilirdi. İlk kayıt olan kullanıcı Admin, diğerleri 403 alır.

## 7. Doğrulama (validation) ve yaşadığımız bir hata
`Dtos/Dtolar.cs` → `KayitIstegi`:
```csharp
public record KayitIstegi(
    [Required(ErrorMessage = "Ad zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad 2-100 karakter olmalıdır.")]
    string Ad, ...);
```
`[ApiController]` sayesinde kurallara uymayan istek metoda **hiç ulaşmadan** 400 döner:
```json
{ "status": 400, "errors": { "Sifre": ["Şifre en az 8 karakter olmalıdır."] } }
```
**Yaşanan hata:** İlk yazımda attribute'ları `[property: Required(...)]` şeklinde yazdım. ASP.NET Core record'larda
attribute'un **constructor parametresinde** olmasını istiyor ve şu hatayı verdi:
`Record type 'KayitIstegi' has validation metadata defined on property 'Sifre' that will be ignored.` Hata mesajını okuyup düzelttim.

## 8. Arayüz (`wwwroot/uygulama.js`)
- **Giriş/Kayıt ekranı:** Oturum yoksa sadece bu görünüyor.
- **Token saklama:** `localStorage`. Sayfa yenilense de giriş kalıcı.
  ⚠️ localStorage'ı sayfadaki her JavaScript okuyabilir; XSS açığı olursa token çalınabilir. Daha güvenli seçenek **HttpOnly cookie**
  (JavaScript okuyamaz). Mobil uygulama da bu API'yi kullanacağı için burada token yaklaşımını seçtik.
- **Her istekte** `Authorization: Bearer ...` başlığı ekleniyor (`api()` fonksiyonu).
- **401 gelirse** (token süresi doldu) otomatik giriş ekranına dönülüyor.

**Yakalanan CSS hatası:** Giriş ekranında menü ve Çıkış düğmesi görünüyordu. `hidden` özniteliği, CSS'teki `display: flex`
kuralı tarafından eziliyordu. `stil.css` başına `[hidden] { display: none !important; }` eklendi.

## 9. İçerik senkronizasyonu (`Data/BaslangicVerisi.cs`)
Ders notları ve terim örnekleri `Konular.TamamlanmaTarihi` sütununa atıf yapıyordu. Bu sütun artık yok, bu yüzden içerik güncellendi.
Önceden seed sadece **boş** alanları dolduruyordu, yani senin veritabanında eski metinler kalırdı. Artık:
- **İçerik kodda yaşar** (başlık, ders notu, terim tanımı, örnekler, görev ve proje metinleri). Her açılışta koddaki hale getirilir.
- **İlerleme veritabanında yaşar.** Kayıtlar Id ile değil **doğal anahtarla** (alan adı + konu sırası, terim adı) eşlendiği ve silinip
  yeniden eklenmediği için ilerleme korunur.

## 10. Bu adımın terimleri
| Terim | Anlamı | Bu projede |
|---|---|---|
| **Authentication / Authorization** | Kimsin? / İznin var mı? | `UseAuthentication()` → `UseAuthorization()` |
| **JWT, Claim** | İmzalı kimlik kartı / içindeki her bilgi | `sub`, `name`, `role` |
| **Bearer Token** | "Taşıyana" yetki veren token | `Authorization: Bearer eyJ...` |
| **Stateless Auth** | Sunucu oturum tutmaz, her istekte token doğrular | JWT |
| **Hash + Salt** | Tek yönlü dönüşüm + rastgele ek | `PasswordHasher` |
| **User Enumeration** | Hangi hesapların var olduğunu hata mesajından çıkarma | Aynı hata mesajı |
| **Fallback Policy** | Varsayılan yetki kuralı | Her endpoint giriş ister |
| **Secure by Default** | Unutulan şey açık değil kapalı kalır | Fallback policy |
| **Role-based Authorization** | Role göre yetki | `[Authorize(Roles = "Admin")]` |
| **Options Pattern** | Ayar bölümünü sınıfa bağlama | `IOptions<JwtAyarlari>` |
| **Fail Fast** | Hatalı ayarda hemen dur | Jwt:Anahtar kontrolü |
| **Extension Method** | Var olan tipe metot ekleme | `User.KullaniciId()` |
| **Composite Key** | Birden fazla sütundan oluşan birincil anahtar | `(KullaniciId, KonuId)` |
| **Junction Table** | Çoka çok ilişkideki bağlantı tablosu | `KonuIlerlemeleri` |
| **Data Migration** | Şema değişirken veriyi taşıma | `EskiIlerleme` |
| **ExecuteDelete** | Okumadan doğrudan DELETE | Görev işaretini kaldırma |
| **409 Conflict** | Kaynak zaten var | Aynı e-postayla kayıt |
| **AsSplitQuery** | Çok Include'u ayrı sorgulara bölme | Seed |

## 11. 🖥️ Bilgisayarında
Çalışan uygulamayı `Ctrl+C` ile durdur:
```powershell
cd ~\Projeler\AliskanlikApp
git pull
cd src\YolHaritasi.Api
dotnet run
```
1. Terminalde `Applying migration '..._KullanicilarVeKisiselIlerleme'` satırını gör.
2. http://localhost:5019 → **Kayıt ol** sekmesinden hesap aç. **İlk kayıt sen olduğun için Admin olursun ve önceki ilerlemen sana aktarılır.**
3. SSMS'te `Kullanicilar` tablosuna bak: şifren yok, sadece hash'i var. `KonuIlerlemeleri` tablosunda eski ilerlemeni gör.
4. F12 → Network → herhangi bir istek → Request Headers → `Authorization: Bearer ...`
5. Çıkış yap, ikinci bir hesap aç: ilerlemesi sıfırdan başlar. Veritabanı sekmesinde SQL çalıştırmayı dene → yetki hatası.
6. Visual Studio'da `YolHaritasi.Api.http` → önce **Giriş yap** isteğini gönder; sonraki istekler token'ı otomatik kullanır.
