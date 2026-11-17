using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using YolHaritasi.Api.Auth;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Hatalar;
using YolHaritasi.Api.Services;

// ============================================================================
//  Program.cs: Uygulamanın başlangıç noktası.
//  İki bölümden oluşur:
//   A) builder.Services...  → SERVİSLERİN KAYDI (Dependency Injection konteyneri)
//   B) app.Use.../Map...    → İSTEK BORU HATTI (middleware pipeline) ve endpoint'ler
// ============================================================================

// ---------------------------------------------------------------------------
//  SERILOG: Başlangıç (bootstrap) logger'ı — Adım 6
//  Uygulama ayarları okunmadan ÖNCE de bir hata olabilir (ör. appsettings bozuk).
//  Onu da yakalayabilmek için en başta basit bir konsol logger'ı kuruyoruz.
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Uygulama başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    // .NET'in yerleşik loglama sistemini (ILogger) Serilog'a yönlendir.
    // Ayarlar appsettings.json → "Serilog" bölümünden okunur: seviyeler, hedefler (konsol + dosya).
    // Kod içindeki ILogger<T> kullanımları DEĞİŞMEDİ; sadece logların nereye ve nasıl yazıldığı değişti.
    builder.Services.AddSerilog((servisler, ayar) => ayar
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(servisler)
        .Enrich.FromLogContext());

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

    // ---------------------------------------------------------------------------
    //  HATA YÖNETİMİ — Adım 6
    // ---------------------------------------------------------------------------
    // PROBLEM DETAILS: Tüm hata cevaplarını standart biçimde (RFC 9457) üret.
    // CustomizeProblemDetails her hata cevabına uygulanır: doğrulama hataları, 404'ler, exception'lar...
    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = ctx =>
        {
            var problem = ctx.ProblemDetails;

            // Trace id: Kullanıcı bu kodu iletirse logda tam olarak o isteği buluruz.
            problem.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;

            // Framework'ün ürettiği İngilizce başlıkları Türkçeleştir
            if (problem.Title == "One or more validation errors occurred.")
                problem.Title = "Gönderilen bilgilerde hata var.";
            else if (problem.Detail is null)   // gövdesiz hata cevapları (UseStatusCodePages'ten gelenler)
                problem.Title = problem.Status switch
                {
                    401 => "Giriş yapman gerekiyor.",
                    403 => "Bu işlem için yetkin yok.",
                    404 => "Aradığın adres bulunamadı.",
                    405 => "Bu adres bu HTTP metodunu desteklemiyor.",
                    _ => problem.Title,
                };

            // Arayüz her hatada "mesaj" alanını okuyor; ProblemDetails'te de olsun.
            if (problem.Detail is not null)
                problem.Extensions["mesaj"] = problem.Detail;
        };
    });

    // Yakalanmayan tüm exception'ları GlobalHataYakalayici karşılasın (Hatalar/GlobalHataYakalayici.cs)
    builder.Services.AddExceptionHandler<GlobalHataYakalayici>();

    // ---------------------------------------------------------------------------
    //  SAĞLIK KONTROLÜ (Health Check) — Adım 6
    //  IIS, yük dengeleyici (load balancer) veya izleme araçları bu adresi düzenli çağırıp
    //  "uygulama ayakta mı, veritabanına ulaşabiliyor mu?" diye bakar.
    // ---------------------------------------------------------------------------
    builder.Services.AddHealthChecks()
        // Veritabanına gerçekten bağlanıp basit bir sorgu çalıştırır
        .AddDbContextCheck<UygulamaDbContext>("veritabani", tags: ["hazirlik"]);

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
    builder.Services.AddScoped<IstatistikServisi>();
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

    // EN BAŞTA: Sonraki tüm middleware'lerde ve controller'larda oluşan exception'ları yakalar.
    // En başta olmalı ki her şeyi sarmalasın.
    app.UseExceptionHandler();

    // 404, 405 gibi gövdesiz hata cevaplarına da ProblemDetails gövdesi ekler.
    app.UseStatusCodePages();

    // İSTEK LOGU: Her istek için TEK satır özet log (metot, yol, durum kodu, süre, kullanıcı).
    // Varsayılan ASP.NET Core istek logları çok gürültülüdür (istek başına ~10 satır); bu onların yerine geçer.
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} → {StatusCode} ({Elapsed:0} ms) Kullanıcı={KullaniciId}";
        options.EnrichDiagnosticContext = (tanilama, context) =>
            tanilama.Set("KullaniciId", context.User.FindFirst(KullaniciUzantilari.IdClaim)?.Value ?? "-");
    });

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

    // SAĞLIK KONTROLLERİ
    //   /saglik/canli  → LIVENESS: Süreç ayakta mı? (hiçbir kontrol çalıştırmaz, hep 200)
    //   /saglik        → READINESS: İstek almaya hazır mı? (veritabanı dahil tüm kontroller)
    // Ayrım önemli: Veritabanı geçici olarak kapalıysa uygulamayı yeniden başlatmak çözüm değildir
    // (liveness başarılı), ama ona trafik göndermemek gerekir (readiness başarısız → 503).
    app.MapHealthChecks("/saglik/canli", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
    app.MapHealthChecks("/saglik", new HealthCheckOptions { ResponseWriter = SaglikCevabiYaz }).AllowAnonymous();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)   // HostAbortedException: "dotnet ef" araçları normal olarak fırlatır
{
    // Uygulama hiç başlayamadıysa (ör. veritabanına ulaşılamadı, ayar eksik) bunu mutlaka logla.
    Log.Fatal(ex, "Uygulama başlatılamadı");
}
finally
{
    // Dosyaya yazılmayı bekleyen logları diske aktar ve kapat.
    Log.CloseAndFlush();
}

// Sağlık cevabını JSON olarak yazar:
// { "durum": "Healthy", "sureMs": 12, "kontroller": [ { "ad": "veritabani", "durum": "Healthy", "sureMs": 11 } ] }
static Task SaglikCevabiYaz(HttpContext context, HealthReport rapor)
{
    context.Response.ContentType = "application/json; charset=utf-8";
    var cevap = new
    {
        durum = rapor.Status.ToString(),
        sureMs = (int)rapor.TotalDuration.TotalMilliseconds,
        kontroller = rapor.Entries.Select(e => new
        {
            ad = e.Key,
            durum = e.Value.Status.ToString(),
            sureMs = (int)e.Value.Duration.TotalMilliseconds,
            // Hata mesajı: sadece hangi bileşenin sorunlu olduğunu söyler; şifre vb. içermez
            hata = e.Value.Exception?.Message,
        }),
    };
    return context.Response.WriteAsync(JsonSerializer.Serialize(cevap));
}
