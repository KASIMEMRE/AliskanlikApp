# Gün 1: Backend nedir? İlk API

⏱️ Tahmini süre: 1–1,5 saat

---

## 📖 1. Terimler (önce bunları oku)

### Büyük resim
Telefonundaki uygulama verileri kendisi saklamaz. İnternetteki bir bilgisayara "bana alışkanlıklarımı ver" diye sorar, o da cevap verir.

```
 📱 Mobil uygulama                         🖥️ Sunucu (bizim .NET kodumuz)
 ┌──────────────┐   İSTEK (request)     ┌──────────────────────┐
 │              │ ───────────────────▶ │  GET /aliskanliklar  │
 │   İSTEMCİ    │                       │        ↓             │
 │   (client)   │ ◀─────────────────── │  listeyi bul, JSON yap│
 └──────────────┘   CEVAP (response)    └──────────────────────┘
                    200 OK + JSON
```

| Terim | Anlamı | Bizim projede |
|---|---|---|
| **Client (istemci)** | İsteği gönderen taraf | Mobil uygulama, tarayıcı, `curl` |
| **Server (sunucu)** | İsteği karşılayıp cevap veren program veya bilgisayar | `AliskanlikApi` |
| **Backend** | Kullanıcının görmediği sunucu tarafı: iş kuralları, veri, güvenlik | Bu kursun ana konusu |
| **Frontend** | Kullanıcının gördüğü taraf | İleride MAUI uygulaması |
| **API** | Application Programming Interface. Programların birbiriyle konuşmasını sağlayan "menü": hangi adrese ne gönderirsen ne alırsın | Bizim endpoint listemiz |
| **HTTP** | İstemci ile sunucunun konuştuğu dil (protokol) | Tüm istekler |
| **Request / Response** | İstek / cevap. Her HTTP konuşması bir istek ve bir cevaptan oluşur | |
| **URL** | Adres. `http://localhost:5000/aliskanliklar/2` | |
| **Host** | Sunucunun adı veya IP'si. `localhost` = "bu bilgisayar" | `localhost` |
| **Port** | Aynı bilgisayardaki programları ayıran kapı numarası (0–65535) | `5000` |
| **Route (yol)** | URL'nin host:port'tan sonraki kısmı | `/aliskanliklar/2` |
| **Endpoint** | Bir route ile bir HTTP metodunun eşleşmesi. "Bu adrese bu metotla gelinirse şu kod çalışır" | `GET /aliskanliklar` |
| **HTTP metodu** | İsteğin amacı. **GET** = oku, **POST** = oluştur, **PUT** = güncelle, **DELETE** = sil | Bugün sadece GET |
| **JSON** | Verinin metin olarak taşınma biçimi: `{"id":1,"ad":"Su iç"}` | Tüm cevaplar |
| **Serialization** | C# nesnesini JSON'a çevirmek (tersi: *deserialization*) | ASP.NET otomatik yapıyor |
| **Status code** | Cevabın sonucunu anlatan sayı | aşağıda |

### HTTP durum kodları (ezberle, her gün göreceksin)
| Kod | Anlam | Ne zaman |
|---|---|---|
| **200** OK | Başarılı | Veri bulundu |
| **201** Created | Oluşturuldu | POST başarılı (Gün 2) |
| **400** Bad Request | İstek hatalı | Eksik/yanlış veri |
| **401** Unauthorized | Kimsin bilmiyorum | Giriş yapılmamış |
| **403** Forbidden | Kim olduğunu biliyorum ama yetkin yok | |
| **404** Not Found | Bulunamadı | Olmayan id |
| **500** Internal Server Error | Sunucuda hata oldu | Kodumuz patladı |

> Kural: **2xx** = başarılı, **4xx** = istemcinin hatası, **5xx** = sunucunun hatası.

### .NET terimleri
| Terim | Anlamı |
|---|---|
| **.NET** | Microsoft'un uygulama platformu: C# derleyicisi, çalışma ortamı (runtime) ve kütüphaneler |
| **SDK** | Geliştirme kiti. Kod yazmak ve derlemek için gerekir (`dotnet` komutu) |
| **Runtime** | Sadece çalıştırmak için gerekir. Sunucuya genelde runtime kurulur |
| **ASP.NET Core** | .NET'in web uygulaması ve API yazma kütüphanesi |
| **Kestrel** | ASP.NET Core'un içinde gelen web sunucusu. `dotnet run` dediğinde isteği dinleyen o. (Gün 15'te IIS'in Kestrel'in önüne nasıl geçtiğini göreceğiz) |
| **Minimal API** | `app.MapGet(...)` ile az kodla endpoint yazma tarzı. İleride Controller tarzını da göreceğiz |
| **NuGet** | .NET'in paket deposu (hazır kütüphaneler) |

---

## 🔍 2. Kodu oku

Sırasıyla aç ve **her yorumu** oku:

1. **`AliskanlikApi/AliskanlikApi.csproj`**: Proje dosyası.
   - `Sdk="Microsoft.NET.Sdk.Web"` → Bu bir web projesi
   - `net10.0` → .NET 10 kullanıyoruz
   - `Nullable` → `null` olabilecek değişkenler için derleyici uyarı versin
   - `ImplicitUsings` → `using System;` gibi satırları otomatik ekle
2. **`AliskanlikApi/Program.cs`**: Uygulamanın tamamı. 5 bölüm var: builder → app → veri → endpoint'ler → run
3. **`AliskanlikApi/Properties/launchSettings.json`**: `dotnet run` dediğinde hangi port ve ortamla açılacağı. `ASPNETCORE_ENVIRONMENT: Development` → geliştirme ortamındayız
4. **`AliskanlikApi/appsettings.json`**: Ayar dosyası (Gün 13'te detaylı)

### İsteğin yolculuğu
`GET /aliskanliklar/2` geldiğinde:
1. Kestrel 5000 portundan isteği alır
2. ASP.NET Core **routing** sistemi `/aliskanliklar/{id:int}` kalıbıyla eşleştirir
3. URL'deki `2`, `int id` parametresine aktarılır (**model binding**)
4. Lambda çalışır, listede `Id == 2` aranır
5. `Results.Ok(bulunan)` → nesne JSON'a çevrilir, **200** kodu ile geri gönderilir

---

## 🛠️ 3. Görevler (Windows bilgisayarında)

**Kurulum (bir kerelik)**
- [ ] [.NET 10 SDK](https://dotnet.microsoft.com/download) kur. Terminalde (PowerShell) `dotnet --version` yaz, `10.x` görmelisin
- [ ] Editör: **Visual Studio 2026 Community** (önerilen, IIS ve debug için en rahatı) veya **VS Code + C# Dev Kit**
- [ ] Git kur ve bu klasörü bilgisayarına al

**Çalıştır**
- [ ] `cd AliskanlikApi` → `dotnet run`
- [ ] Terminalde yazan adresi tarayıcıda aç (ör. `http://localhost:5138`)
- [ ] Şunları dene ve cevapları incele:
  - `/`
  - `/aliskanliklar`
  - `/aliskanliklar/2`
  - `/aliskanliklar/99` → 404
  - `/aliskanliklar/abc` → neden 404? (ipucu: `:int`)
- [ ] Tarayıcıda **F12 → Network** sekmesini aç, sayfayı yenile. Status, Headers ve Response'u bul. **Bu çok önemli bir alışkanlık!**

**Kendin yaz** (kodu değiştir, `Ctrl+C` ile durdur, tekrar `dotnet run`)
- [ ] Listeye kendi alışkanlığını ekle
- [ ] `GET /aliskanliklar/sayi` endpoint'i yaz: kaç alışkanlık olduğunu döndürsün
- [ ] `GET /aliskanliklar/en-uzun-seri` endpoint'i yaz: serisi en yüksek alışkanlığı döndürsün (ipucu: `OrderByDescending`)
- [ ] ⭐ Bonus: `GET /aliskanliklar/ara?kelime=su` → adında o kelime geçenleri döndürsün. `?kelime=` kısmına **query string** denir; lambda'ya `(string kelime)` parametresi eklemen yeterli

---

## ❓ 4. Kendini test et
Cevaplarını yaz, ben kontrol edeyim:
1. Client ile server arasındaki fark ne? Tarayıcı hangisi?
2. `http://localhost:5000/aliskanliklar/2` adresinde host, port ve route hangisi?
3. Endpoint ile route arasındaki fark ne?
4. 404 ile 500 arasındaki fark ne? Hangisi "bizim hatamız"?
5. Uygulamayı kapatıp açınca eklediğin alışkanlığa ne oluyor? Neden?
6. `app.Run()` satırını `MapGet` satırlarının **üstüne** taşırsan ne olur? (Tahmin et, sonra dene!)

---
➡️ **Yarın (Gün 2):** POST, PUT, DELETE ile alışkanlık ekleme, güncelleme, silme. Yani **CRUD**.
