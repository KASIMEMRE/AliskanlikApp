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

    // ---- Adım 4: kullanıcılar ve kişisel ilerleme ----
    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<KonuIlerleme> KonuIlerlemeleri => Set<KonuIlerleme>();
    public DbSet<GorevIlerleme> GorevIlerlemeleri => Set<GorevIlerleme>();
    public DbSet<ProjeAdimIlerleme> ProjeAdimIlerlemeleri => Set<ProjeAdimIlerleme>();

    // ---- Adım 5: notlar ve çalışma süresi ----
    public DbSet<Not> Notlar => Set<Not>();
    public DbSet<CalismaOturumu> CalismaOturumlari => Set<CalismaOturumu>();

    /// <summary>
    /// CONVENTION (kural) ayarı: Modeldeki TÜM DateTime property'lerine bir kerede uygulanır.
    ///
    /// Sorun: Tarihleri UTC olarak kaydediyoruz (DateTime.UtcNow). Ama SQL Server'ın datetime2 tipi
    /// saat dilimi bilgisi tutmaz. EF Core okurken DateTimeKind.Unspecified döner, JSON'da da sonda "Z"
    /// olmaz ("2026-11-17T12:10:22"). Tarayıcı bunu YEREL saat sanar → Türkiye'de 3 saat kayma!
    /// Çözüm: VALUE CONVERTER ile "veritabanından okunan her tarih UTC'dir" diyoruz → JSON'da "...Z" olur.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDonusturucu>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcNullableDonusturucu>();
    }

    private class UtcDonusturucu() : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
        yazarken => yazarken.Kind == DateTimeKind.Local ? yazarken.ToUniversalTime() : yazarken,
        okurken => DateTime.SpecifyKind(okurken, DateTimeKind.Utc));

    private class UtcNullableDonusturucu() : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
        yazarken => yazarken.HasValue && yazarken.Value.Kind == DateTimeKind.Local ? yazarken.Value.ToUniversalTime() : yazarken,
        okurken => okurken.HasValue ? DateTime.SpecifyKind(okurken.Value, DateTimeKind.Utc) : okurken);

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

        // ---- Adım 4: kullanıcılar ve kişisel ilerleme ----
        modelBuilder.Entity<Kullanici>(e =>
        {
            e.Property(x => x.Ad).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.SifreHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Rol).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();   // aynı e-postayla iki hesap açılamaz
        });

        // HasKey(x => new { ... }) → COMPOSITE PRIMARY KEY (iki sütunlu birincil anahtar)
        modelBuilder.Entity<KonuIlerleme>(e =>
        {
            e.HasKey(x => new { x.KullaniciId, x.KonuId });
            e.HasOne(x => x.Kullanici).WithMany(k => k.KonuIlerlemeleri).HasForeignKey(x => x.KullaniciId);
            e.HasOne(x => x.Konu).WithMany(k => k.Ilerlemeler).HasForeignKey(x => x.KonuId);
        });

        modelBuilder.Entity<GorevIlerleme>(e =>
        {
            e.HasKey(x => new { x.KullaniciId, x.GorevId });
            e.HasOne(x => x.Gorev).WithMany(g => g.Ilerlemeler).HasForeignKey(x => x.GorevId);
        });

        modelBuilder.Entity<ProjeAdimIlerleme>(e =>
        {
            e.HasKey(x => new { x.KullaniciId, x.ProjeAdimiId });
            e.HasOne(x => x.ProjeAdimi).WithMany(a => a.Ilerlemeler).HasForeignKey(x => x.ProjeAdimiId);
        });

        // ---- Adım 5: notlar ve çalışma süresi ----
        modelBuilder.Entity<Not>(e =>
        {
            e.Property(x => x.Icerik).HasMaxLength(4000).IsRequired();
            // En sık sorgu: "bu kullanıcının bu konudaki notları" → iki sütunlu index
            e.HasIndex(x => new { x.KullaniciId, x.KonuId });
        });

        modelBuilder.Entity<CalismaOturumu>(e =>
        {
            e.Property(x => x.Aciklama).HasMaxLength(500);
            // CHECK CONSTRAINT: Veritabanı seviyesinde kural. Uygulama kodu atlansa bile SQL Server 0 veya
            // negatif dakikayı, ya da 10 saatten uzun oturumu kabul etmez. (Doğrulamanın son savunma hattı)
            e.ToTable(t => t.HasCheckConstraint("CK_CalismaOturumlari_Dakika", "[Dakika] BETWEEN 1 AND 600"));
            // İstatistik sorguları kullanıcı + tarihe göre filtreliyor
            e.HasIndex(x => new { x.KullaniciId, x.Baslangic });
        });
    }
}
