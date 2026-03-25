using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMalIdToManga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MalId",
                table: "Catalogo",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_MalId",
                table: "Catalogo",
                column: "MalId",
                unique: true,
                filter: "\"MalId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Catalogo_MalId",
                table: "Catalogo");

            migrationBuilder.DropColumn(
                name: "MalId",
                table: "Catalogo");
        }
    }
}
