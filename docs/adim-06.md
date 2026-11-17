# Adım 6: Global Hata Yönetimi, Serilog ile Loglama ve Health Check

> **Bu adımda ne yaptık?**
> - Tüm hatalar **tek bir yerde** yakalanıyor (`GlobalHataYakalayici`). Controller'lardaki try/catch blokları kalktı.
> - Hata cevapları standart **ProblemDetails** biçiminde ve Türkçe. Beklenmeyen hatada kullanıcıya detay değil **trace id** gidiyor.
> - **Serilog:** Loglar hem konsola hem `logs/` klasöründeki **günlük dosyalara** yazılıyor. Her istek için tek satır özet.
> - **Health check:** `/saglik` (veritabanı dahil) ve `/saglik/canli`. Veritabanı kapanınca 503 dönüyor.

Bu adım, uygulamayı **sunucuda çalıştırmaya hazırlıyor**. Canlıda bir sorun olduğunda elinde sadece loglar ve sağlık kontrolü olur.

---

## 1. Global hata yönetimi (`Hatalar/GlobalHataYakalayici.cs`)

### Önce
Her controller kendi hatasını yakalıyordu:
```csharp
try { return await hesap.KayitAsync(istek); }
catch (HesapHatasi ex) { return Conflict(new HataDto(ex.Message)); }
```
Yeni bir controller'da catch yazmayı unutursan kullanıcıya **stack trace'li** bir 500 sayfası gidebilirdi.

### Şimdi
```csharp
// Controller: sadece iş
public async Task<ActionResult<OturumDto>> Kayit(KayitIstegi istek) => await hesap.KayitAsync(istek);

// Servis: hata durumunu ve kodunu söyler
throw new HesapHatasi("Bu e-posta adresiyle zaten bir hesap var.", StatusCodes.Status409Conflict);
```
`HesapHatasi` ve `SorguHatasi`, `UygulamaHatasi`'ndan türüyor. `GlobalHataYakalayici` iki tür hatayı ayırıyor:

| Hata türü | Örnek | Kullanıcıya | Log |
|---|---|---|---|
| **Beklenen** (`UygulamaHatasi`) | Yanlış şifre, yasak SQL | Mesaj + kendi kodu (400/401/409) | Warning |
| **Beklenmeyen** (diğer her şey) | Veritabanı kapalı, NullReference | Genel mesaj + **trace id**, 500 | Error + tam stack trace |

**Neden detay gösterilmiyor?** Stack trace; dosya yollarını, kütüphane sürümlerini, SQL'i gösterir. Bu bilgiler saldırgana yol gösterir (**information disclosure**).

### Pipeline'daki yeri (`Program.cs`)
```csharp
app.UseExceptionHandler();     // EN BAŞTA: sonraki her şeyi sarmalar
app.UseStatusCodePages();      // gövdesiz 401/404 cevaplarına da ProblemDetails gövdesi ekler
app.UseSerilogRequestLogging();
...
```

## 2. ProblemDetails (RFC 9457)
Bütün hata cevapları artık aynı biçimde:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "İstek işlenemedi.",
  "status": 409,
  "detail": "Bu e-posta adresiyle zaten bir hesap var.",
  "traceId": "0HNQ4KIUET2NR:00000001",
  "mesaj": "Bu e-posta adresiyle zaten bir hesap var."
}
```
`AddProblemDetails(... CustomizeProblemDetails ...)` her cevaba **traceId** ekliyor ve framework'ün İngilizce başlıklarını Türkçeleştiriyor.
`mesaj` alanı, arayüzün eski hata okuma koduyla uyumlu kalsın diye eklendi.

## 3. Serilog: yapılandırılmış loglama
**Neden?** .NET'in yerleşik logları konsola yazar ve uygulama kapanınca kaybolur. Sunucuda gerekenler:
- **Dosyaya** yazmak, günlük yeni dosya açmak, eski dosyaları silmek (14 gün)
- Her logun **özelliklerini** (KullaniciId, RequestPath...) ayrı alanlar olarak saklamak

**Kod değişmedi:** `ILogger<T>` kullanan her satır aynı. Sadece logların nereye ve nasıl gittiği değişti (`builder.Services.AddSerilog(...)`).

Ayarlar `appsettings.json` → `"Serilog"` bölümünde:
```json
"MinimumLevel": { "Default": "Information", "Override": { "Microsoft.AspNetCore": "Warning" } },
"WriteTo": [ { "Name": "Console" }, { "Name": "File", "Args": { "path": "logs/yolharitasi-.log", "rollingInterval": "Day" } } ]
```
Geliştirmede (`appsettings.Development.json`) seviye `Debug` ve EF Core SQL komutları açık.

**İstek logu:** Her HTTP isteği için tek satır:
```
[12:17:33 INF] HTTP GET /saglik → 200 (36 ms) Kullanıcı=-
[12:17:32 ERR] HTTP GET /api/gelistirici/hata → 500 (12 ms) Kullanıcı=-
```
**Dosya logu** (`src/YolHaritasi.Api/logs/yolharitasi-20261117.log`), özellikleri JSON olarak içerir:
```
2026-11-17 12:17:32.628 +00:00 [ERR] ...GlobalHataYakalayici Beklenmeyen hata: GET /api/gelistirici/hata {"RequestId":"0HNQ4KIUET2NR:00000001",...}
System.IndexOutOfRangeException: Index was outside the bounds of the array.
   at YolHaritasi.Api.Controllers.GelistiriciController.Hata() in GelistiriciController.cs:line 25
```
Kullanıcı sana trace id'yi (`0HNQ4KIUET2NR:00000001`) söylerse, dosyada aratıp hatanın tam yerini bulursun.

**Bootstrap logger ve `Log.Fatal`:** Uygulama hiç başlayamazsa (ayar eksik, veritabanına ulaşılamıyor) bu da loglanır. IIS'te "site açılmıyor" sorunlarında ilk bakacağın yer burası.

## 4. Health check
```csharp
builder.Services.AddHealthChecks().AddDbContextCheck<UygulamaDbContext>("veritabani");
app.MapHealthChecks("/saglik/canli", new() { Predicate = _ => false });   // liveness
app.MapHealthChecks("/saglik", new() { ResponseWriter = SaglikCevabiYaz });  // readiness
```
| Adres | Soru | Veritabanı kapalıyken |
|---|---|---|
| `/saglik/canli` | **Liveness:** Süreç ayakta mı? | 200: Uygulamayı yeniden başlatmak çözüm değil |
| `/saglik` | **Readiness:** İstek almaya hazır mı? | **503**: Bu sunucuya trafik gönderme |

**Test ettik:** SQL Server'ı durdurduk → `/saglik` 503 Unhealthy, `/saglik/canli` 200, sözlük isteği detaysız 500 + trace id, uygulama **çökmedi**.
SQL Server açılınca hiçbir şeyi yeniden başlatmadan Healthy'ye döndü. Uygulamada **🗄️ Veritabanı** sekmesinin başında sistem sağlığı görünüyor.

## 5. Deneme endpoint'leri (sadece Development)
`Controllers/GelistiriciController.cs`:
- `GET /api/gelistirici/hata` → bilerek 500. Cevaptaki traceId'yi `logs/` dosyasında ara.
- `GET /api/gelistirici/log` → 6 seviyede log yazar. Hangilerinin göründüğünü `MinimumLevel` belirler.

## 6. Bu adımın terimleri
| Terim | Anlamı | Bu projede |
|---|---|---|
| **Global Exception Handling** | Tüm hataları tek yerde yakalama | `IExceptionHandler` |
| **ProblemDetails (RFC 9457)** | Standart HTTP hata cevabı biçimi | Tüm hata cevapları |
| **Trace ID / Correlation ID** | Bir isteği loglarda bulmayı sağlayan kimlik | `traceId` |
| **Information Disclosure** | Hata detayıyla sistem bilgisi sızdırma | 500'de detay yok |
| **Structured Logging** | Log özelliklerini ayrı alanlar olarak saklama | `{KullaniciId}` |
| **Sink** | Logun yazıldığı hedef | Console, File |
| **Rolling File** | Belirli aralıklarla yeni log dosyası | `rollingInterval: Day` |
| **Log Level** | Trace < Debug < Information < Warning < Error < Fatal | `MinimumLevel` |
| **Request Logging** | Her istek için özet log satırı | `UseSerilogRequestLogging` |
| **Health Check** | Uygulamanın sağlık durumunu bildiren adres | `/saglik` |
| **Liveness / Readiness** | "Ayakta mı?" / "Trafik almaya hazır mı?" | `/saglik/canli`, `/saglik` |
| **503 Service Unavailable** | Hizmet geçici olarak kullanılamıyor | Veritabanı kapalı |

## 7. 🖥️ Bilgisayarında
```powershell
cd ~\Projeler\AliskanlikApp
git pull
cd src\YolHaritasi.Api
dotnet run
```
1. Konsolda yeni log biçimini gör: `[12:17:33 INF] HTTP GET /api/bugun → 200 (15 ms) Kullanıcı=1`
2. `src\YolHaritasi.Api\logs\` klasöründeki günlük dosyayı aç.
3. Tarayıcıda http://localhost:5019/api/gelistirici/hata → cevaptaki `traceId`'yi kopyala, log dosyasında ara.
4. http://localhost:5019/saglik → Healthy.
5. **Arıza deneyi:** Uygulama açıkken `sqllocaldb stop MSSQLLocalDB -k` → `/saglik` 503 olmalı. Sonra `sqllocaldb start MSSQLLocalDB` → tekrar Healthy.
