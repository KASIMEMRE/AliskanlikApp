# Adım 3: Sözlüğe Gerçek Kullanım Örnekleri

> **Bu adımda ne yaptık?** Sözlükteki 145 terimin her birine iki şey eklendi:
> - **🧭 Nerede kullanılır?** Terim gerçek projelerde hangi durumda karşına çıkar.
> - **💻 Örnek kullanım:** Çalışan bir C#, SQL, PowerShell, HTTP veya JSON örneği. Mümkün olan her yerde
>   **bu projenin kendi kodundan** alındı (dosya adı yorumda yazıyor), böylece terimi hem okuyup hem projede bulabilirsin.

## Uygulamada nerede görürsün?
- **📖 Sözlük:** Terim ara. 5 veya daha az sonuç varsa örnekler açık gelir; daha fazlaysa "💻 Gerçek kullanım örneği"ne tıkla.
- **📅 Bugün → ③ Terimler:** Terimin adına tıkla; tanım, kullanım yeri ve örnek kod açılır.
- Her kod kutusunda **Kopyala** düğmesi var. SQL örneklerinin SELECT olanlarını **🗄️ Veritabanı** sekmesindeki SQL Pratik Alanı'nda çalıştırabilirsin.

## Kodda ne değişti?

### Üçüncü migration: `TerimKullanimOrnekleri`
`Entities/Terim.cs` dosyasına üç property eklendi:
```csharp
public string? Kullanim { get; set; }    // nvarchar(1000)
public string? KodOrnegi { get; set; }   // nvarchar(max)
public string? KodDili { get; set; }     // nvarchar(20): "csharp", "sql", "powershell"...
```
`dotnet ef migrations add TerimKullanimOrnekleri` → `ALTER TABLE Terimler ADD ...` üreten migration.
Üçü de nullable, çünkü tabloda zaten 145 satır var (Adım 2'deki "var olan tabloya sütun ekleme" kuralı).

### İçerik dosyaları
```
Data/BaslangicVerisi.TerimOrnekleri1.cs   ← C# Temelleri + Backend terimleri
Data/BaslangicVerisi.TerimOrnekleri2.cs   ← SQL + Sunucu + Yapay Zeka terimleri
```
Her kayıt: `["Terim adı"] = T("Nerede kullanılır", "dil", """ kod """)`.

İçinde `"""` geçen kod örneklerinde (C#'ın kendi raw string'ini gösteren örnekler) sınırlayıcı **dört tırnak** (`""""`) yapıldı.
Raw string literal kuralı: Sınırlayıcı, içerikteki en uzun tırnak dizisinden uzun olmalı.

### Aynı adlı iki terim: "Index"
C# Temelleri'nde **Index** = listedeki sıra numarası, SQL'de **Index** = veritabanı dizini. Sadece ada göre eşlersek
ikisi aynı örneği alırdı. `BaslangicVerisi.EkleAsync` önce **terimin kendi alanının** sözlüğüne bakıyor:
```csharp
var (once, sonra) = terim.Konu.Alan.Ad is "C# Temelleri" or "Backend"
    ? (TerimOrnekleri1, TerimOrnekleri2)
    : (TerimOrnekleri2, TerimOrnekleri1);
```
Burada iki yeni C# özelliği var: **tuple deconstruction** `var (a, b) = ...` ve **pattern matching** `is "x" or "y"`.

### Arayüz
`wwwroot/uygulama.js` içindeki `terimOrnekHtml()` fonksiyonu hem Sözlük hem rehber tarafından kullanılıyor (kod tekrarı yok).
**Kopyala** düğmeleri tek bir `document.addEventListener("click", ...)` ile yönetiliyor. Buna **event delegation** denir:
Sayfaya sonradan eklenen düğmeler için tek tek dinleyici bağlamaya gerek kalmaz.

## Bu adımın terimleri
| Terim | Anlamı | Bu projede |
|---|---|---|
| **Tuple Deconstruction** | Bir tuple'ı tek satırda değişkenlere ayırmak | `var (once, sonra) = ...` |
| **Pattern Matching** | Değeri bir kalıba göre kontrol etmek | `is "C# Temelleri" or "Backend"` |
| **ThenInclude** | İlişkinin ilişkisini de yüklemek (Terim → Konu → Alan) | `BaslangicVerisi.EkleAsync` |
| **Event Delegation** | Çok sayıda öğe için tek bir üst dinleyici | Kopyala düğmeleri |
| **Clipboard API** | Tarayıcının panoya yazma özelliği | `navigator.clipboard.writeText` |

## 🖥️ Bilgisayarında
Çalışan uygulamayı `Ctrl+C` ile durdur, sonra:
```powershell
cd ~\Projeler\AliskanlikApp
git pull
cd src\YolHaritasi.Api
dotnet run
```
Veritabanını silmene gerek yok. Terminalde `Applying migration '..._TerimKullanimOrnekleri'` satırını göreceksin;
örnekler mevcut terimlere otomatik eklenir, ilerlemen korunur.
