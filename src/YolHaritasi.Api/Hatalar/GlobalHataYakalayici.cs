using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace YolHaritasi.Api.Hatalar;

/// <summary>
/// Uygulamanın kendi hata tipi. "Kullanıcıya gösterilebilir" hatalar bundan türer ve
/// hangi HTTP durum koduna dönüşeceklerini kendileri söyler.
/// Controller'larda try/catch yazmak yerine sadece "throw new ...Hatasi(...)" yazarız;
/// GlobalHataYakalayici bunu uygun HTTP cevabına çevirir.
/// </summary>
public abstract class UygulamaHatasi(string mesaj, int durumKodu) : Exception(mesaj)
{
    public int DurumKodu { get; } = durumKodu;
}

/// <summary>
/// GLOBAL HATA YÖNETİMİ (IExceptionHandler, .NET 8+).
///
/// Uygulamanın HERHANGİ bir yerinde yakalanmayan her exception buraya gelir (UseExceptionHandler middleware'i sayesinde).
/// İki tür hata var:
///  1) UygulamaHatasi (beklenen hatalar): "E-posta zaten kayıtlı", "SQL'de DELETE yasak"...
///     → Mesaj kullanıcıya gösterilir, kendi durum kodu döner (400, 401, 409...). Warning seviyesinde loglanır.
///  2) Diğer her şey (beklenmeyen hatalar): NullReferenceException, veritabanı kapalı...
///     → Detay ASLA kullanıcıya gösterilmez (stack trace saldırgana sistem hakkında bilgi verir).
///       Sadece genel bir mesaj + TRACE ID döner. Error seviyesinde TÜM detayıyla loglanır.
///       Kullanıcı trace id'yi söylerse logda tam olarak o hatayı bulursun.
///
/// Cevap biçimi: PROBLEM DETAILS (RFC 9457): HTTP API'lerinde hata cevapları için standart JSON biçimi.
///   { "type": "...", "title": "...", "status": 409, "detail": "...", "traceId": "00-abc..." }
/// </summary>
public class GlobalHataYakalayici(
    IProblemDetailsService problemDetails,
    ILogger<GlobalHataYakalayici> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception hata, CancellationToken iptal)
    {
        ProblemDetails problem;

        if (hata is UygulamaHatasi uygulamaHatasi)
        {
            logger.LogWarning("İş kuralı hatası: {Mesaj} ({DurumKodu}) {Yol}",
                hata.Message, uygulamaHatasi.DurumKodu, context.Request.Path);

            problem = new ProblemDetails
            {
                Status = uygulamaHatasi.DurumKodu,
                Title = "İstek işlenemedi.",
                Detail = hata.Message,
            };
        }
        else if (hata is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            // Kullanıcı sayfayı kapattı / isteği iptal etti. Hata değil; bilgi olarak logla, cevap yazma.
            logger.LogInformation("İstek istemci tarafından iptal edildi: {Yol}", context.Request.Path);
            return true;
        }
        else
        {
            // {@...} değil, exception'ı İLK parametre olarak veriyoruz: Serilog stack trace'i ayrı alanda saklar.
            logger.LogError(hata, "Beklenmeyen hata: {Metot} {Yol}", context.Request.Method, context.Request.Path);

            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Beklenmeyen bir hata oluştu.",
                Detail = "Sunucuda bir sorun oluştu. Sorun devam ederse bu hata kodunu ilet: " + context.TraceIdentifier,
            };
        }

        context.Response.StatusCode = problem.Status!.Value;

        // ProblemDetailsService: Program.cs'teki AddProblemDetails ayarlarını (traceId, mesaj alanı) uygular ve JSON yazar.
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem,
            Exception = hata,
        });
    }
}
