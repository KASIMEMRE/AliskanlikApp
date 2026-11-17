using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Dtos;

namespace YolHaritasi.Api.Controllers;

/// <summary>Terim sözlüğü.</summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Terim Sözlüğü")]   // API arayüzünde (Scalar) bu grubun başlığı
public class TerimlerController(UygulamaDbContext db) : ControllerBase
{
    /// <summary>Terim sözlüğünde arama. Kelime verilmezse tüm terimler alfabetik gelir.</summary>
    // GET /api/terimler            → hepsi
    // GET /api/terimler?ara=key    → adında veya tanımında "key" geçenler
    // [FromQuery] → Değer URL'deki QUERY STRING'den (? sonrası) okunur.
    [HttpGet]
    public async Task<List<TerimAramaDto>> Ara([FromQuery] string? ara)
    {
        // IQueryable: Sorgu henüz ÇALIŞMADI, sadece tarif ediliyor.
        // Aşağıda koşula göre Where ekleyebiliyoruz; ToListAsync() dendiğinde tek bir SQL'e dönüşüp çalışır.
        IQueryable<Entities.Terim> sorgu = db.Terimler.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(ara))
        {
            // Contains → SQL'de LIKE '%ara%' olur.
            // EF Core değeri SQL'e PARAMETRE olarak gönderir (@p0); metin olarak yapıştırmaz.
            // Bu sayede SQL INJECTION saldırısına karşı korunmuş oluruz.
            sorgu = sorgu.Where(t => t.Ad.Contains(ara) || t.Tanim.Contains(ara));
        }

        return await sorgu
            .OrderBy(t => t.Ad)
            .Select(t => new TerimAramaDto(t.Id, t.Ad, t.Tanim, t.Ornek, t.Kullanim, t.KodOrnegi, t.KodDili, t.KonuId, t.Konu.Baslik, t.Konu.Alan.Ad))
            .ToListAsync();
    }
}
