using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEditoras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EditoraId",
                table: "Catalogo",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Editoras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Key = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Editoras", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_EditoraId",
                table: "Catalogo",
                column: "EditoraId");

            migrationBuilder.CreateIndex(
                name: "IX_Editoras_Key",
                table: "Editoras",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Catalogo_Editoras_EditoraId",
                table: "Catalogo",
                column: "EditoraId",
                principalTable: "Editoras",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Catalogo_Editoras_EditoraId",
                table: "Catalogo");

            migrationBuilder.DropTable(
                name: "Editoras");

            migrationBuilder.DropIndex(
                name: "IX_Catalogo_EditoraId",
                table: "Catalogo");

            migrationBuilder.DropColumn(
                name: "EditoraId",
                table: "Catalogo");
        }
    }
}
