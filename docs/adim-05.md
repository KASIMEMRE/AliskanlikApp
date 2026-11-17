# Adım 5: Notlar, Çalışma Süresi, İstatistikler ve Doğrulama

> **Bu adımda ne yaptık?**
> - Her konuya **kişisel not** (ekle, düzenle, sil): tam bir **CRUD** örneği.
> - **⏱️ Çalışma zamanlayıcısı:** Başlat/Bitir ya da süreyi elle gir. Sayfa kapansa da süre devam eder.
> - **📊 İstatistik** sekmesi: günlük seri 🔥, son 7 gün, son 14 günün grafiği, alanlara göre dağılım.
> - Doğrulama **üç katmanda**: tarayıcı, API (DataAnnotations + iş kuralları), veritabanı (CHECK constraint).

## Uygulamada nerede?
- **📅 Bugün** (veya bir konuyu açtığında): Rehber kartının altında **⏱️ Çalışma süresi** ve **📝 Notlarım**.
- **📊 İstatistik** sekmesi.

## 1. Yeni tablolar (`Entities/NotVeCalisma.cs`)
```
Kullanicilar ──1:*── Notlar ──*:1── Konular
Kullanicilar ──1:*── CalismaOturumlari ──*:1── Konular
```
Migration: `NotlarVeCalismaOturumlari`. İçinde iki yeni şey var:
- **İki sütunlu index:** `IX_Notlar_KullaniciId_KonuId`. En sık sorgu "bu kullanıcının bu konudaki notları" olduğu için.
- **CHECK constraint:** `CK_CalismaOturumlari_Dakika` → `[Dakika] BETWEEN 1 AND 600`

## 2. REST ve CRUD (`Controllers/NotlarController.cs`)
| İşlem | HTTP | Başarılı cevap |
|---|---|---|
| Listele | `GET /api/konular/5/notlar` | 200 |
| Oluştur | `POST /api/konular/5/notlar` | **201 Created** + `Location` başlığı |
| Güncelle | `PUT /api/notlar/12` | 204 |
| Sil | `DELETE /api/notlar/12` | 204 |

Not, bir konunun **alt kaynağı** olduğu için listeleme ve oluşturma `/konular/{id}/notlar` altında.
Güncelleme ve silme ise notun kendi Id'siyle yapılıyor.

## 3. Güvenlik: IDOR ve sahiplik kontrolü
**IDOR** (Insecure Direct Object Reference): Kullanıcı URL'deki Id'yi değiştirip başkasının verisine erişebiliyorsa bu açık vardır.
`DELETE /api/notlar/12` → 12, Ayşe'nin notuysa ve Emre silebiliyorsa sistem açıktır.

Her sorguda kullanıcı şartı var:
```csharp
db.Notlar.SingleOrDefaultAsync(n => n.Id == id && n.KullaniciId == User.KullaniciId());
```
Test sonucu: Ayşe, Emre'nin notunu göremedi, güncelleyemedi, silemedi → **404**.
Neden 403 değil? 403 "böyle bir not var ama senin değil" demektir; bu bile bilgi sızdırır.

## 4. Üç katmanlı doğrulama
| Katman | Nerede | Örnek | Neden? |
|---|---|---|---|
| Tarayıcı | `uygulama.js` | 700 dk → "1 ile 600 arasında gir" | Hızlı geri bildirim. **Güvenlik sağlamaz**, atlatılabilir |
| API: DataAnnotations | `Dtos/Dtolar.cs` | `[Range(1, 600)]` → 400 | Kurallara uymayan istek metoda ulaşmaz |
| API: iş kuralı | `CalismaController` | Gelecek tarih → 400, olmayan konu → 404 | Attribute ile ifade edilemeyen kurallar |
| Veritabanı | CHECK constraint | SQL ile `Dakika = 0` → reddedildi | Son savunma hattı: API atlansa bile veri bozulmaz |

Testte SSMS'ten doğrudan `INSERT ... Dakika = 0` denedim:
`The INSERT statement conflicted with the CHECK constraint "CK_CalismaOturumlari_Dakika"`

## 5. İstatistik: SQL'de mi, C#'ta mı hesaplanır? (`Services/IstatistikServisi.cs`)
**Toplama ve gruplama SQL'de:**
```csharp
await oturumlar.GroupBy(c => c.Konu.Alan.Ad)
               .Select(g => new { Alan = g.Key, Dakika = g.Sum(c => c.Dakika) })
               .ToDictionaryAsync(...);
// → SELECT a.Ad, SUM(c.Dakika) FROM CalismaOturumlari c JOIN Konular k ... JOIN Alanlar a ... GROUP BY a.Ad
```
Sadece 5 satır sonuç gelir, binlerce oturum belleğe çekilmez.

**Satır satır mantık C#'ta:** "Arka arkaya kaç gün çalıştın?" (seri) sorusunu SQL'de yazmak mümkün ama okunması çok zor.
Sadece tarih ve dakika sütunları çekilip C#'ta `HashSet<DateOnly>` ile hesaplanıyor.

## 6. Saat dilimi (UTC) hatası ve çözümü
Tarihler veritabanına **UTC** olarak yazılıyor (`DateTime.UtcNow`). Ama SQL Server'ın `datetime2` tipi saat dilimi tutmaz.
EF Core okurken tarihin UTC olduğunu bilmiyordu ve JSON'da sonda `Z` olmadan gönderiyordu: `"2026-11-17T12:10:22"`.
Tarayıcı bunu **yerel saat** sanıyordu, yani Türkiye'de not tarihleri 3 saat geri görünecekti.

Çözüm `UygulamaDbContext.ConfigureConventions`: Bir **value converter** ile "okunan her tarih UTC'dir" dedik.
Artık JSON'da `"2026-11-17T12:10:22Z"` geliyor ve tarayıcı doğru yerel saate çeviriyor. Şema değişmediği için migration gerekmedi.

"Bugün" ve "gün" kavramları ise Türkiye saatine göre (`Europe/Istanbul`) hesaplanıyor. Gece 01:00'de çalışırsan o gün sayılır, UTC'deki önceki gün değil.

## 7. Arayüz
- **Zamanlayıcı:** Başlangıç zamanı `localStorage`'da tutuluyor. Sekmeyi kapatıp açsan da süre devam eder. Aynı anda tek zamanlayıcı çalışır.
- **Notlar:** Düzenle → textarea'ya `value` ile atanıyor (`innerHTML` değil), böylece içerik HTML olarak yorumlanmaz. Testte `<script>` içeren not düz metin olarak göründü.
- **Grafik:** Tek seri, ince sütunlar. Üzerine gelince (veya klavyeyle odaklanınca) tarih ve süre görünüyor. "Tablo olarak göster" ile aynı veri tabloda.

## 8. Bu adımın terimleri
| Terim | Anlamı | Bu projede |
|---|---|---|
| **CRUD** | Create, Read, Update, Delete | Notlar |
| **201 Created + Location** | Yeni kaynak oluşturuldu ve adresi | `POST .../notlar` |
| **Alt kaynak (sub-resource)** | Başka bir kaynağa ait kaynak | `/konular/5/notlar` |
| **IDOR** | Id değiştirerek başkasının verisine erişme açığı | Sahiplik kontrolü |
| **CHECK Constraint** | Veritabanı seviyesinde değer kuralı | `Dakika BETWEEN 1 AND 600` |
| **Defense in Depth** | Birden çok bağımsız doğrulama katmanı | Tarayıcı + API + veritabanı |
| **Aggregate (SUM, COUNT)** | Toplama fonksiyonları | İstatistik |
| **GROUP BY** | Gruplayıp toplama | Alanlara göre süre |
| **Value Converter** | Veritabanı ↔ C# değer dönüşümü | UTC tarih dönüştürücü |
| **Convention** | Tüm modele birden uygulanan kural | `ConfigureConventions` |
| **DateTimeKind** | Tarihin UTC / Local / Unspecified olduğu bilgisi | `DateTime.SpecifyKind` |
| **DateOnly** | Saatsiz tarih tipi | Günlük gruplama |
| **HashSet** | Tekrarsız, hızlı arama yapılan koleksiyon | Seri hesabı |

## 9. 🖥️ Bilgisayarında
```powershell
cd ~\Projeler\AliskanlikApp
git pull
cd src\YolHaritasi.Api
dotnet run
```
1. Bir konu aç → **▶ Çalışmaya başla** → birkaç dakika sonra **■ Bitir ve kaydet**.
2. **📝 Notlarım**'a kendi cümlelerinle bir not yaz.
3. **📊 İstatistik** sekmesinde grafiği gör.
4. SSMS'te `CalismaOturumlari` tablosunu aç, sonra şunu dene (hata vermeli):
   ```sql
   INSERT INTO CalismaOturumlari (Baslangic, Dakika, KullaniciId, KonuId) VALUES (SYSUTCDATETIME(), 0, 1, 1);
   ```
