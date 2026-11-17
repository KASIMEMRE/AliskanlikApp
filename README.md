# 🌱 AliskanlikApp — Öğrenerek Geliştirme Projesi

Amaç: Oyunlaştırılmış bir **alışkanlık takip uygulaması** yaparken şunları en baştan öğrenmek:
**C# / .NET backend**, **Windows sunucu yönetimi (IIS, Windows Service, Registry)** ve **yapay zeka entegrasyonu**.

**Mobil:** .NET MAUI. Böylece her şey tek dil (C#) ile yazılır ve enerji backend'e kalır.

## Çalışma şekli
- Her gün `gunler/GunXX.md` dosyasında: **📖 Terimler → 🔍 Kodu oku → 🛠️ Görevler → ❓ Kendini test et**
- Bir gün = yaklaşık 1–2 saat. Acele yok; anlamadığın bir şey kalırsa bir sonraki güne geçme, bana sor.
- Kod içindeki yorumlar da dersin parçası, onları atlama.

## Yol haritası

### 1. Hafta — Web API'nin temelleri
| Gün | Konu |
|---|---|
| 1 | İstemci/sunucu, HTTP, endpoint, JSON. İlk API (GET) |
| 2 | POST/PUT/DELETE, CRUD, request body, status kodları |
| 3 | Postman/.http dosyaları, Swagger/OpenAPI, doğrulama (validation) |
| 4 | Dependency Injection, servis katmanı, interface |
| 5 | Controller yapısı, katmanlı mimari, klasör düzeni |

### 2. Hafta — Veritabanı
| Gün | Konu |
|---|---|
| 6 | SQL Server kurulumu (Windows), SQL temelleri |
| 7 | Entity Framework Core, DbContext, ilk migration |
| 8 | İlişkiler: Kullanıcı ↔ Alışkanlık ↔ Günlük kayıt |
| 9 | LINQ sorguları, seri (streak) hesaplama |
| 10 | DTO, mapping, sayfalama (pagination) |

### 3. Hafta — Güvenlik ve konfigürasyon
| Gün | Konu |
|---|---|
| 11 | Kimlik doğrulama vs yetkilendirme, şifre hashleme |
| 12 | JWT ile giriş, token yapısı |
| 13 | appsettings, environment'lar, secrets, Options pattern |
| 14 | Loglama (Serilog), global hata yönetimi (middleware) |

### 4. Hafta — Windows sunucu tarafı 🖥️
| Gün | Konu |
|---|---|
| 15 | IIS kurulumu, Hosting Bundle, ilk publish |
| 16 | Application Pool, kimlik (identity), binding, port |
| 17 | HTTPS sertifikası, `web.config`, sık IIS hataları (500.19, 502.5…) |
| 18 | Worker Service: günlük hatırlatma ve seri sıfırlama işi |
| 19 | Windows Service olarak kurma (`sc.exe`), servis hesapları, Event Viewer |
| 20 | Registry: regedit, servis ayarlarını Registry'den okuma, izinler |
| 21 | PowerShell ile otomasyon, Task Scheduler, firewall |

### 5. Hafta — Yapay zeka 🤖
| Gün | Konu |
|---|---|
| 22 | LLM nedir: token, prompt, model, API anahtarı |
| 23 | .NET'ten bir LLM API'sini çağırmak (`Microsoft.Extensions.AI`) |
| 24 | "AI Koç": kullanıcının verisine göre kişisel öneri (prompt mühendisliği) |
| 25 | Yapılandırılmış çıktı (JSON), tool calling: AI'ın API'mizi kullanması |
| 26 | Embedding ve vektör arama, basit RAG |
| 27 | Maliyet, hız sınırı (rate limit), güvenlik (prompt injection) |

### 6. Hafta — Gerçek zamanlı ve mobil 📱
| Gün | Konu |
|---|---|
| 28 | SignalR: canlı arkadaş sıralaması |
| 29–32 | .NET MAUI: ekranlar, API'ye bağlanma, giriş, bildirim |
| 33+ | Docker, CI/CD, cache (Redis), health check, performans |

## Çalıştırma
```bash
cd AliskanlikApi
dotnet run
```
Tarayıcıda: `http://localhost:5xxx/aliskanliklar` (port, terminalde yazar)
