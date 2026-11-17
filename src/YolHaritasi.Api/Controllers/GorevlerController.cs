using Microsoft.AspNetCore.Mvc;
using YolHaritasi.Api.Services;

namespace YolHaritasi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GorevlerController(IIlerlemeServisi ilerleme) : ControllerBase
{
    /// <summary>İsteğin GÖVDESİNDE (body) gelecek JSON'un şekli: { "tamamlandi": true }</summary>
    public record GorevDurumIstegi(bool Tamamlandi);

    /// <summary>Görevin tamamlanma durumunu değiştirir.</summary>
    // PATCH /api/gorevler/7    gövde: { "tamamlandi": true }
    // PATCH = kaynağın sadece bir kısmını güncelle. (PUT tamamını değiştirmek içindir.)
    // [FromBody] → Bu parametre URL'den değil, isteğin gövdesindeki JSON'dan okunur (deserialization).
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> DurumDegistir(int id, [FromBody] GorevDurumIstegi istek)
    {
        var bulundu = await ilerleme.GorevDurumuDegistirAsync(id, istek.Tamamlandi);
        return bulundu ? NoContent() : NotFound();
    }
}
