namespace YolHaritasi.Api.Entities;

/// <summary>
/// Proje = Bir alanda öğrendiklerini uygulayacağın, adım adım ilerleyen pratik çalışma.
/// Tablo adı: Projeler
/// </summary>
public class Proje
{
    public int Id { get; set; }
    public string Baslik { get; set; } = "";
    public string Aciklama { get; set; } = "";
    public string Zorluk { get; set; } = "";   // "Kolay", "Orta", "Zor"
    public int Sira { get; set; }

    public int AlanId { get; set; }
    public Alan Alan { get; set; } = null!;

    public List<ProjeAdimi> Adimlar { get; set; } = [];
}

/// <summary>
/// ProjeAdimi = Projenin tek bir adımı. Tablo adı: ProjeAdimlari
/// </summary>
public class ProjeAdimi
{
    public int Id { get; set; }
    public int Sira { get; set; }
    public string Aciklama { get; set; } = "";

    // İpucu zorunlu değil → nullable (NULL olabilir)
    public string? Ipucu { get; set; }
    public bool Tamamlandi { get; set; }

    public int ProjeId { get; set; }
    public Proje Proje { get; set; } = null!;
}
