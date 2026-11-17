using Microsoft.EntityFrameworkCore;
using YolHaritasi.Api.Entities;

namespace YolHaritasi.Api.Data;

/// <summary>
/// DbContext = Uygulama ile veritabanı arasındaki KÖPRÜ.
/// - Hangi tabloların olduğunu (DbSet'ler) bilir
/// - C# LINQ sorgularını SQL'e çevirir
/// - Nesnelerdeki değişiklikleri takip eder (CHANGE TRACKING) ve SaveChanges() deyince
///   bunları INSERT / UPDATE / DELETE komutlarına çevirip veritabanına gönderir
///
/// Bu yaklaşıma ORM (Object-Relational Mapping) denir: nesneler ↔ tablolar eşlemesi.
/// Entity Framework Core, .NET'in ORM'idir.
/// </summary>
public class UygulamaDbContext(DbContextOptions<UygulamaDbContext> options) : DbContext(options)
{
    // Her DbSet bir tabloyu temsil eder. Özellik adı = tablo adı.
    public DbSet<Alan> Alanlar => Set<Alan>();
    public DbSet<Konu> Konular => Set<Konu>();
    public DbSet<Terim> Terimler => Set<Terim>();
    public DbSet<Gorev> Gorevler => Set<Gorev>();
    public DbSet<Kaynak> Kaynaklar => Set<Kaynak>();
    public DbSet<Proje> Projeler => Set<Proje>();
    public DbSet<ProjeAdimi> ProjeAdimlari => Set<ProjeAdimi>();

    /// <summary>
    /// FLUENT API: Tabloların detaylı ayarlarını (sütun uzunluğu, index, ilişki) burada yapıyoruz.
    /// Buradaki her ayar MIGRATION'a yansır ve sonunda SQL'e (CREATE TABLE ...) dönüşür.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alan>(e =>
        {
            // Sınır vermezsek string → nvarchar(max) olur. Sınır vermek hem veri kalitesi
            // hem performans (index'lenebilirlik) için iyidir.
            e.Property(x => x.Ad).HasMaxLength(100).IsRequired();
            e.Property(x => x.Aciklama).HasMaxLength(500);

            // UNIQUE INDEX: Aynı adla iki alan eklenemez. SQL Server bunu kendisi engeller.
            e.HasIndex(x => x.Ad).IsUnique();
        });

        modelBuilder.Entity<Konu>(e =>
        {
            e.Property(x => x.Baslik).HasMaxLength(200).IsRequired();
            e.Property(x => x.Ozet).HasMaxLength(1000);

            // İLİŞKİ: Bir Konu'nun bir Alan'ı var, bir Alan'ın çok Konu'su var.
            // OnDelete(Cascade): Alan silinirse ona bağlı konular da silinsin.
            e.HasOne(x => x.Alan)
             .WithMany(a => a.Konular)
             .HasForeignKey(x => x.AlanId)
             .OnDelete(DeleteBehavior.Cascade);

            // Composite (bileşik) index: Bir alanda aynı sıra numarası iki kez olamaz.
            e.HasIndex(x => new { x.AlanId, x.Sira }).IsUnique();
        });

        modelBuilder.Entity<Terim>(e =>
        {
            e.Property(x => x.Ad).HasMaxLength(100).IsRequired();
            e.Property(x => x.Tanim).HasMaxLength(1000).IsRequired();
            e.Property(x => x.Ornek).HasMaxLength(1000);
            e.Property(x => x.Kullanim).HasMaxLength(1000);
            e.Property(x => x.KodDili).HasMaxLength(20);
            // KodOrnegi için sınır yok → nvarchar(max)

            // Sözlükte terim adına göre arama yapacağız; index aramayı hızlandırır.
            e.HasIndex(x => x.Ad);
        });

        modelBuilder.Entity<Gorev>(e =>
        {
            e.Property(x => x.Aciklama).HasMaxLength(500).IsRequired();
        });

        // ---- Adım 2'de eklenen tablolar ----
        modelBuilder.Entity<Kaynak>(e =>
        {
            e.Property(x => x.Baslik).HasMaxLength(200).IsRequired();
            e.Property(x => x.Url).HasMaxLength(500).IsRequired();
            e.Property(x => x.Site).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Proje>(e =>
        {
            e.Property(x => x.Baslik).HasMaxLength(200).IsRequired();
            e.Property(x => x.Aciklama).HasMaxLength(2000);
            e.Property(x => x.Zorluk).HasMaxLength(20);
        });

        modelBuilder.Entity<ProjeAdimi>(e =>
        {
            e.ToTable("ProjeAdimlari");
            e.Property(x => x.Aciklama).HasMaxLength(1000).IsRequired();
            e.Property(x => x.Ipucu).HasMaxLength(1000);
        });
    }
}
