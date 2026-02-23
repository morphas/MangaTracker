using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLancadoEditora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Editora",
                table: "Catalogo",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EditoraKey",
                table: "Catalogo",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LancadoNoBrasil",
                table: "Catalogo",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Editora",
                table: "Catalogo");

            migrationBuilder.DropColumn(
                name: "EditoraKey",
                table: "Catalogo");

            migrationBuilder.DropColumn(
                name: "LancadoNoBrasil",
                table: "Catalogo");
        }
    }
}
