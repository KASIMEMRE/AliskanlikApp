using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YolHaritasi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class DersNotlariKaynaklarProjeler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DersNotu",
                table: "Konular",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Kaynaklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Site = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    KonuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kaynaklar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kaynaklar_Konular_KonuId",
                        column: x => x.KonuId,
                        principalTable: "Konular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Zorluk = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    AlanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projeler_Alanlar_AlanId",
                        column: x => x.AlanId,
                        principalTable: "Alanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjeAdimlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Ipucu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tamamlandi = table.Column<bool>(type: "bit", nullable: false),
                    ProjeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjeAdimlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjeAdimlari_Projeler_ProjeId",
                        column: x => x.ProjeId,
                        principalTable: "Projeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kaynaklar_KonuId",
                table: "Kaynaklar",
                column: "KonuId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjeAdimlari_ProjeId",
                table: "ProjeAdimlari",
                column: "ProjeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projeler_AlanId",
                table: "Projeler",
                column: "AlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kaynaklar");

            migrationBuilder.DropTable(
                name: "ProjeAdimlari");

            migrationBuilder.DropTable(
                name: "Projeler");

            migrationBuilder.DropColumn(
                name: "DersNotu",
                table: "Konular");
        }
    }
}
