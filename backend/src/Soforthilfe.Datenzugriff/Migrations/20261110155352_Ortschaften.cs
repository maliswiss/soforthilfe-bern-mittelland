using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Soforthilfe.Datenzugriff.Migrations
{
    /// <inheritdoc />
    public partial class Ortschaften : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ortschaften",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Postleitzahl = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Zusatzziffer = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Gemeinde = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    BfsNummer = table.Column<int>(type: "integer", nullable: false),
                    Kanton = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Adressenanteil = table.Column<double>(type: "double precision", nullable: false),
                    Sprache = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Standort = table.Column<Point>(type: "geography (Point, 4326)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ortschaften", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ortschaften_Postleitzahl_Zusatzziffer_BfsNummer",
                table: "Ortschaften",
                columns: new[] { "Postleitzahl", "Zusatzziffer", "BfsNummer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ortschaften_Standort",
                table: "Ortschaften",
                column: "Standort")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ortschaften");
        }
    }
}
