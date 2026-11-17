using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YolHaritasi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class KullanicilarVeKisiselIlerleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ================================================================
            //  ELLE EKLENDİ: VERİ KAYBETMEDEN TAŞIMA (data migration)
            //  EF Core sadece şemayı bilir; aşağıdaki DropColumn satırları sütunları
            //  içindeki veriyle birlikte siler. Silmeden ÖNCE eski ilerlemeyi geçici
            //  bir tabloya kopyalıyoruz. İlk kayıt olan kullanıcı bu ilerlemeyi alır
            //  (Services/HesapServisi.cs → EskiIlerlemeyiTasiAsync) ve tablo silinir.
            //  migrationBuilder.Sql(...) → migration içinde ham SQL çalıştırır.
            // ================================================================
            migrationBuilder.Sql("""
                CREATE TABLE EskiIlerleme (Tur nvarchar(20) NOT NULL, KayitId int NOT NULL, Tarih datetime2 NOT NULL);

                INSERT INTO EskiIlerleme (Tur, KayitId, Tarih)
                SELECT 'Konu', Id, TamamlanmaTarihi FROM Konular WHERE TamamlanmaTarihi IS NOT NULL;

                INSERT INTO EskiIlerleme (Tur, KayitId, Tarih)
                SELECT 'Gorev', Id, SYSUTCDATETIME() FROM Gorevler WHERE Tamamlandi = 1;

                INSERT INTO EskiIlerleme (Tur, KayitId, Tarih)
                SELECT 'ProjeAdimi', Id, SYSUTCDATETIME() FROM ProjeAdimlari WHERE Tamamlandi = 1;
                """);

            migrationBuilder.DropColumn(
                name: "Tamamlandi",
                table: "ProjeAdimlari");

            migrationBuilder.DropColumn(
                name: "TamamlanmaTarihi",
                table: "Konular");

            migrationBuilder.DropColumn(
                name: "Tamamlandi",
                table: "Gorevler");

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SifreHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GorevIlerlemeleri",
                columns: table => new
                {
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    GorevId = table.Column<int>(type: "int", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GorevIlerlemeleri", x => new { x.KullaniciId, x.GorevId });
                    table.ForeignKey(
                        name: "FK_GorevIlerlemeleri_Gorevler_GorevId",
                        column: x => x.GorevId,
                        principalTable: "Gorevler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GorevIlerlemeleri_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KonuIlerlemeleri",
                columns: table => new
                {
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    KonuId = table.Column<int>(type: "int", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KonuIlerlemeleri", x => new { x.KullaniciId, x.KonuId });
                    table.ForeignKey(
                        name: "FK_KonuIlerlemeleri_Konular_KonuId",
                        column: x => x.KonuId,
                        principalTable: "Konular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KonuIlerlemeleri_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjeAdimIlerlemeleri",
                columns: table => new
                {
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    ProjeAdimiId = table.Column<int>(type: "int", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeAdimIlerlemeleri", x => new { x.KullaniciId, x.ProjeAdimiId });
                    table.ForeignKey(
                        name: "FK_ProjeAdimIlerlemeleri_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjeAdimIlerlemeleri_ProjeAdimlari_ProjeAdimiId",
                        column: x => x.ProjeAdimiId,
                        principalTable: "ProjeAdimlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GorevIlerlemeleri_GorevId",
                table: "GorevIlerlemeleri",
                column: "GorevId");

            migrationBuilder.CreateIndex(
                name: "IX_KonuIlerlemeleri_KonuId",
                table: "KonuIlerlemeleri",
                column: "KonuId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_Email",
                table: "Kullanicilar",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjeAdimIlerlemeleri_ProjeAdimiId",
                table: "ProjeAdimIlerlemeleri",
                column: "ProjeAdimiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GorevIlerlemeleri");

            migrationBuilder.DropTable(
                name: "KonuIlerlemeleri");

            migrationBuilder.DropTable(
                name: "ProjeAdimIlerlemeleri");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.AddColumn<bool>(
                name: "Tamamlandi",
                table: "ProjeAdimlari",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TamamlanmaTarihi",
                table: "Konular",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Tamamlandi",
                table: "Gorevler",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
