using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Auth;
using YolHaritasi.Api.Data;
using YolHaritasi.Api.Dtos;
using YolHaritasi.Api.Entities;

namespace YolHaritasi.Api.Controllers;

/// <summary>
/// Konulara kişisel notlar: TAM CRUD örneği (Create, Read, Update, Delete).
///
/// SAHİPLİK KONTROLÜ: Her sorguda "KullaniciId == giriş yapan kullanıcı" şartı var.
/// Başkasının notunun Id'sini tahmin edip PUT/DELETE göndersen bile 404 alırsın.
/// Neden 403 değil de 404? 403 "böyle bir not var ama senin değil" demek olur; bu bile bilgi sızdırır.
/// Bu açığın adı IDOR (Insecure Direct Object Reference): OWASP'ın en sık görülen açıklarından.
/// </summary>
[ApiController]
[Tags("Notlar")]
public class NotlarController(UygulamaDbContext db) : ControllerBase
{
    /// <summary>Bir konudaki notlarım (en yeni önce).</summary>
    // GET /api/konular/5/notlar
    [HttpGet("api/konular/{konuId:int}/notlar")]
    public async Task<List<NotDto>> Listele(int konuId)
    {
        var kullaniciId = User.KullaniciId();
        return await db.Notlar
            .AsNoTracking()
            .Where(n => n.KonuId == konuId && n.KullaniciId == kullaniciId)
            .OrderByDescending(n => n.OlusturmaTarihi)
            .Select(n => new NotDto(n.Id, n.KonuId, n.Icerik, n.OlusturmaTarihi, n.GuncellemeTarihi))
            .ToListAsync();
    }

    /// <summary>Konuya not ekler. Başarılıysa 201 Created ve yeni notu döner.</summary>
    // POST /api/konular/5/notlar    gövde: { "icerik": "LINQ'te Where filtreler, Select dönüştürür." }
    [HttpPost("api/konular/{konuId:int}/notlar")]
    public async Task<ActionResult<NotDto>> Ekle(int konuId, NotIstegi istek)
    {
        if (!await db.Konular.AnyAsync(k => k.Id == konuId))
            return NotFound(new HataDto($"{konuId} numaralı konu bulunamadı."));

        var not = new Not { KonuId = konuId, KullaniciId = User.KullaniciId(), Icerik = istek.Icerik.Trim() };
        db.Notlar.Add(not);
        await db.SaveChangesAsync();

        // 201 Created: REST'te yeni kaynak oluşturulunca dönülen kod.
        // Location başlığına yeni kaynağın adresi yazılır: Location: /api/konular/5/notlar
        return Created($"/api/konular/{konuId}/notlar", new NotDto(not.Id, not.KonuId, not.Icerik, not.OlusturmaTarihi, null));
    }

    /// <summary>Notu günceller (sadece kendi notun).</summary>
    // PUT /api/notlar/12    gövde: { "icerik": "yeni metin" }
    [HttpPut("api/notlar/{id:int}")]
    public async Task<IActionResult> Guncelle(int id, NotIstegi istek)
    {
        var not = await db.Notlar.SingleOrDefaultAsync(n => n.Id == id && n.KullaniciId == User.KullaniciId());
        if (not is null)
            return NotFound(new HataDto("Not bulunamadı."));

        not.Icerik = istek.Icerik.Trim();
        not.GuncellemeTarihi = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Notu siler (sadece kendi notun).</summary>
    // DELETE /api/notlar/12
    [HttpDelete("api/notlar/{id:int}")]
    public async Task<IActionResult> Sil(int id)
    {
        var kullaniciId = User.KullaniciId();
        var silinen = await db.Notlar
            .Where(n => n.Id == id && n.KullaniciId == kullaniciId)
            .ExecuteDeleteAsync();   // → DELETE FROM Notlar WHERE Id = @id AND KullaniciId = @k

        return silinen == 0 ? NotFound(new HataDto("Not bulunamadı.")) : NoContent();
    }
}
