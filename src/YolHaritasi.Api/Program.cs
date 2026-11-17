using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using YolHaritasi.Api.Auth;
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

// Interface'siz kayıt da mümkün: "Biri VeritabaniServisi isterse yenisini ver."
builder.Services.AddScoped<VeritabaniServisi>();
builder.Services.AddScoped<HesapServisi>();
builder.Services.AddSingleton<TokenServisi>();   // durum tutmuyor, herkes aynı nesneyi kullanabilir

// ---------------------------------------------------------------------------
//  KİMLİK DOĞRULAMA (Authentication) ve YETKİLENDİRME (Authorization) — Adım 4
// ---------------------------------------------------------------------------
// OPTIONS PATTERN: appsettings'teki "Jwt" bölümünü JwtAyarlari sınıfına bağla.
builder.Services.Configure<JwtAyarlari>(builder.Configuration.GetSection(JwtAyarlari.Bolum));
var jwt = builder.Configuration.GetSection(JwtAyarlari.Bolum).Get<JwtAyarlari>() ?? new JwtAyarlari();

// FAIL FAST: Ayar eksikse uygulama hiç başlamasın; hatayı canlıda ilk istekte değil, açılışta görelim.
if (jwt.Anahtar.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Anahtar ayarı eksik veya 32 karakterden kısa. Geliştirmede appsettings.Development.json'a, " +
        "sunucuda ortam değişkenine (Jwt__Anahtar) yaz.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)   // "Bearer" token ile kimlik doğrula
    .AddJwtBearer(options =>
    {
        // Claim adlarını olduğu gibi bırak ("sub" → "sub"). Aksi halde .NET bunları uzun URL'li adlara çevirir.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,            ValidIssuer = jwt.Yayinci,
            ValidateAudience = true,          ValidAudience = jwt.Hedef,
            ValidateLifetime = true,          // süresi dolmuş token → 401
            ValidateIssuerSigningKey = true,  IssuerSigningKey = TokenServisi.ImzaAnahtari(jwt),
            NameClaimType = KullaniciUzantilari.AdClaim,
            RoleClaimType = KullaniciUzantilari.RolClaim,   // [Authorize(Roles = "Admin")] bu claim'e bakar
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization(options =>
{
    // FALLBACK POLICY: Üzerinde hiçbir [Authorize]/[AllowAnonymous] olmayan TÜM endpoint'ler giriş ister.
    // "Varsayılan olarak kapalı" (secure by default): Yeni bir endpoint yazıp [Authorize] koymayı unutsan bile korunur.
    // Herkese açık olması gerekenlere açıkça [AllowAnonymous] yazıyoruz (HesapController'a bak).
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

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
    // Dokümantasyon sayfaları giriş istemesin (fallback policy'den muaf)
    app.MapOpenApi().AllowAnonymous();               // GET /openapi/v1.json
    app.MapScalarApiReference(o => o.WithTitle("Yol Haritası API")).AllowAnonymous();   // GET /scalar
}

app.UseHttpsRedirection();          // http ile gelen isteği https'e yönlendirir

// ARAYÜZ: wwwroot klasöründeki dosyaları (HTML, CSS, JavaScript) tarayıcıya sunar.
// UseDefaultFiles: Kök adrese (/) gelen isteği wwwroot/index.html'e çevirir.
// UseStaticFiles: Dosyayı diskten okuyup gönderir. Sıra önemli: önce DefaultFiles, sonra StaticFiles.
app.UseDefaultFiles();
app.UseStaticFiles();

// Sıra önemli: önce "kimsin?" (token'ı oku), sonra "iznin var mı?" (policy'leri uygula).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();               // Controller'lardaki [Route]/[HttpGet] tanımlarını endpoint olarak bağla

app.Run();
