using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YolHaritasi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class NotlarVeCalismaOturumlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalismaOturumlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslangic = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Dakika = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    KonuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalismaOturumlari", x => x.Id);
                    table.CheckConstraint("CK_CalismaOturumlari_Dakika", "[Dakika] BETWEEN 1 AND 600");
                    table.ForeignKey(
                        name: "FK_CalismaOturumlari_Konular_KonuId",
                        column: x => x.KonuId,
                        principalTable: "Konular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalismaOturumlari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Icerik = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    KonuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notlar_Konular_KonuId",
                        column: x => x.KonuId,
                        principalTable: "Konular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notlar_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalismaOturumlari_KonuId",
                table: "CalismaOturumlari",
                column: "KonuId");

            migrationBuilder.CreateIndex(
                name: "IX_CalismaOturumlari_KullaniciId_Baslangic",
                table: "CalismaOturumlari",
                columns: new[] { "KullaniciId", "Baslangic" });

            migrationBuilder.CreateIndex(
                name: "IX_Notlar_KonuId",
                table: "Notlar",
                column: "KonuId");

            migrationBuilder.CreateIndex(
                name: "IX_Notlar_KullaniciId_KonuId",
                table: "Notlar",
                columns: new[] { "KullaniciId", "KonuId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalismaOturumlari");

            migrationBuilder.DropTable(
                name: "Notlar");
        }
    }
}
