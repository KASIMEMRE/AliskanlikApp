# 🗺️ Yol Haritası: C# · Backend · SQL · Sunucu · Yapay Zeka

Bu repo iki şeydir:

1. **Bir uygulama:** C#, Backend, SQL, Windows Sunucu ve Yapay Zeka öğrenme yol haritasını sunar.
   "Bugün ne çalışmalıyım?", Türkçe ders notları, W3Schools / Microsoft Learn kaynakları, terim sözlüğü,
   proje görevleri, veritabanı ekranı ve SQL pratik alanı içerir.
2. **Bir ders kitabı:** Uygulama adım adım geliştiriliyor. Her adımda kodu okuyarak
   **neyin neden yapıldığını** takip ediyorsun.

## 📚 Nasıl takip edilir?
- Her geliştirme adımı `docs/` klasöründe anlatılır. **Sırayla oku:**
  - [Adım 1: Proje yapısı, SQL Server ve ilk API](docs/adim-01.md)
  - [Adım 2: Ders notları, kaynaklar, projeler ve veritabanı ekranı](docs/adim-02.md)
- Kodda ne değiştiğini görmek için GitHub'da **Commits** sekmesine bak. Her commit bir adımdır.
- Kod dosyalarındaki yorumlar da anlatımın parçası.

## 🧱 Mimari (şu anki hali)
```
 📱 İstemci (wwwroot web arayüzü / Scalar / ileride mobil)
        │  HTTP + JSON
        ▼
 ┌─────────────────── YolHaritasi.Api (ASP.NET Core) ──────────────────┐
 │  Controllers  →  Services (iş kuralları)  →  DbContext (EF Core)    │
 └──────────────────────────────────────────────────────────┬──────────┘
                                                            │ SQL
                                                            ▼
                                                   🗄️ SQL Server
              (Alanlar · Konular · Terimler · Gorevler · Kaynaklar · Projeler · ProjeAdimlari)
```

## 🛣️ Geliştirme planı
| Adım | Uygulamaya eklenecek | Öğrenilecek |
|---|---|---|
| ✅ 1 | Proje, SQL Server, tablolar, yol haritası verisi, okuma/tamamlama API'leri | Web API, Controller, DI, EF Core, Migration, DTO |
| ✅ 2 | C# Temelleri, ders notları, kaynaklar, projeler, veritabanı ekranı, SQL pratik alanı | İkinci migration, ADO.NET, ExecuteUpdate, güvenlik katmanları |
| 3 | Kullanıcı kaydı/girişi, herkesin kendi ilerlemesi | Authentication, JWT, hash, many-to-many, veri taşıma |
| 3+ | Not alma, çalışma süresi kaydı, istatistikler, doğrulama | CRUD, validation, GROUP BY |
| 4 | Global hata yönetimi, yapılandırılmış loglama, health check | Middleware, Serilog, ProblemDetails |
| 5 | IIS'e yayın | Publish, App Pool, binding, HTTPS, web.config |
| 6 | Günlük hatırlatma Windows Service'i + Registry'den ayar | Worker Service, sc.exe, Registry, Event Viewer |
| 7 | 🤖 AI Mentor: terimi basitçe anlat, quiz üret | LLM, prompt, Microsoft.Extensions.AI |
| 8 | 🤖 AI'ın uygulamayı kullanması, anlamsal terim arama | Tool calling, embedding, RAG |
| 9 | 📱 Mobil uygulama (.NET MAUI) | MAUI, HttpClient, MVVM |
| 10 | Docker, CI/CD, cache | Container, GitHub Actions, Redis |

## ▶️ Çalıştırma (Windows)
Kurulum ve detaylar: [docs/adim-01.md](docs/adim-01.md#️-bilgisayarında-çalıştır)
```bash
cd src/YolHaritasi.Api
dotnet run
```
Tarayıcıda **http://localhost:5019** → Türkçe uygulama ekranı açılır (Bugün · Alanlar · Sözlük).
API'yi doğrudan denemek için: **http://localhost:5019/scalar**
