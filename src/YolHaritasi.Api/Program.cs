using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Services;

// ============================================================================
//  Program.cs: Uygulamanın başlangıç noktası.
//  İki bölümden oluşur:
//   A) builder.Services...  → SERVİSLERİN KAYDI (Dependency Injection konteyneri)
//   B) app.Use.../Map...    → İSTEK BORU HATTI (middleware pipeline) ve endpoint'ler
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
//  A) SERVİSLER
// ---------------------------------------------------------------------------

// Controller sınıflarını (Controllers klasörü) bul ve kullanıma hazırla.
builder.Services.AddControllers();

// OpenAPI: API'mizin "kullanım kılavuzunu" (hangi endpoint'ler var, ne alır, ne döner)
// otomatik üretir. /openapi/v1.json adresinden görülebilir.
// DOCUMENT TRANSFORMER: Üretilen dokümanın başlığını ve açıklamasını Türkçe yapıyoruz.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((dokuman, context, ct) =>
    {
        dokuman.Info.Title = "Yol Haritası API";
        dokuman.Info.Description =
            "Backend, SQL, Sunucu ve Yapay Zeka öğrenme yol haritası. " +
            "Bugünün konusunu, terim sözlüğünü, görevleri ve ilerlemeni sunar.";
        return Task.CompletedTask;
    });
});

// VERİTABANI: DbContext'i DI sistemine kaydet.
// CONNECTION STRING appsettings.json → "ConnectionStrings:Varsayilan" ayarından okunur.
// AddDbContext varsayılan olarak SCOPED kaydeder: her HTTP isteği kendi DbContext'ini alır,
// istek bitince DbContext ve veritabanı bağlantısı kapatılır.
builder.Services.AddDbContext<UygulamaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Varsayilan")));

// Kendi servisimiz: "Biri IIlerlemeServisi isterse ona IlerlemeServisi ver."
// Scoped: Her istek için bir tane (çünkü içinde Scoped olan DbContext kullanıyor).
builder.Services.AddScoped<IIlerlemeServisi, IlerlemeServisi>();

var app = builder.Build();

// ---------------------------------------------------------------------------
//  Veritabanını hazırla (sadece GELİŞTİRME ortamında)
// ---------------------------------------------------------------------------
// Migrate(): Veritabanı yoksa oluşturur, uygulanmamış MIGRATION'ları sırayla uygular.
// Production'da (canlı ortamda) bunu otomatik yapmak risklidir; orada migration'ları
// kontrollü şekilde ayrı bir adımda uygularız (Sunucu konularında göreceğiz).
if (app.Environment.IsDevelopment())
{
    // CreateScope: Uygulama başlarken henüz bir HTTP isteği yok. Scoped servis (DbContext)
    // alabilmek için kendimiz geçici bir "scope" açıyoruz.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<UygulamaDbContext>();
    await db.Database.MigrateAsync();
    await BaslangicVerisi.EkleAsync(db);
}

// ---------------------------------------------------------------------------
//  B) İSTEK BORU HATTI (MIDDLEWARE PIPELINE)
//  Her HTTP isteği bu satırlardan YUKARIDAN AŞAĞIYA geçer. Sıra önemlidir.
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // GET /openapi/v1.json
    app.MapScalarApiReference(o => o.WithTitle("Yol Haritası API"));   // GET /scalar → API'yi deneyebileceğin arayüz
}

app.UseHttpsRedirection();          // http ile gelen isteği https'e yönlendirir

// ARAYÜZ: wwwroot klasöründeki dosyaları (HTML, CSS, JavaScript) tarayıcıya sunar.
// UseDefaultFiles: Kök adrese (/) gelen isteği wwwroot/index.html'e çevirir.
// UseStaticFiles: Dosyayı diskten okuyup gönderir. Sıra önemli: önce DefaultFiles, sonra StaticFiles.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();               // Controller'lardaki [Route]/[HttpGet] tanımlarını endpoint olarak bağla

app.Run();
