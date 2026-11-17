using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YolHaritasi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class TerimKullanimOrnekleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KodDili",
                table: "Terimler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KodOrnegi",
                table: "Terimler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kullanim",
                table: "Terimler",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KodDili",
                table: "Terimler");

            migrationBuilder.DropColumn(
                name: "KodOrnegi",
                table: "Terimler");

            migrationBuilder.DropColumn(
                name: "Kullanim",
                table: "Terimler");
        }
    }
}
