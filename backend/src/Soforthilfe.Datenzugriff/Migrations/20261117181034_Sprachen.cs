using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Soforthilfe.Datenzugriff.Migrations
{
    /// <inheritdoc />
    public partial class Sprachen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sprachen",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Aktiv = table.Column<bool>(type: "boolean", nullable: false),
                    Texte = table.Column<string>(type: "jsonb", nullable: false),
                    AnzahlTexte = table.Column<int>(type: "integer", nullable: false),
                    GeaendertAm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GeaendertVonId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sprachen", x => x.Code);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sprachen");
        }
    }
}
