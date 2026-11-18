using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Soforthilfe.Datenzugriff.Migrations
{
    /// <inheritdoc />
    public partial class Vorschlaege : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vorschlaege",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnfrageId = table.Column<Guid>(type: "uuid", nullable: false),
                    HelferprofilId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rang = table.Column<int>(type: "integer", nullable: false),
                    Punktzahl = table.Column<double>(type: "double precision", nullable: false),
                    DistanzMeter = table.Column<int>(type: "integer", nullable: false),
                    TeilpunkteDistanz = table.Column<double>(type: "double precision", nullable: false),
                    TeilpunkteFaehigkeit = table.Column<double>(type: "double precision", nullable: false),
                    TeilpunkteBewertung = table.Column<double>(type: "double precision", nullable: false),
                    TeilpunkteVerfuegbarkeit = table.Column<double>(type: "double precision", nullable: false),
                    TeilpunkteErfahrung = table.Column<double>(type: "double precision", nullable: false),
                    GewichtDistanz = table.Column<double>(type: "double precision", nullable: false),
                    GewichtFaehigkeit = table.Column<double>(type: "double precision", nullable: false),
                    GewichtBewertung = table.Column<double>(type: "double precision", nullable: false),
                    GewichtVerfuegbarkeit = table.Column<double>(type: "double precision", nullable: false),
                    GewichtErfahrung = table.Column<double>(type: "double precision", nullable: false),
                    PartnerBevorzugt = table.Column<bool>(type: "boolean", nullable: false),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vorschlaege", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vorschlaege_Anfragen_AnfrageId",
                        column: x => x.AnfrageId,
                        principalTable: "Anfragen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vorschlaege_Helferprofile_HelferprofilId",
                        column: x => x.HelferprofilId,
                        principalTable: "Helferprofile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vorschlaege_Regionen_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regionen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vorschlaege_AnfrageId_HelferprofilId",
                table: "Vorschlaege",
                columns: new[] { "AnfrageId", "HelferprofilId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vorschlaege_HelferprofilId",
                table: "Vorschlaege",
                column: "HelferprofilId");

            migrationBuilder.CreateIndex(
                name: "IX_Vorschlaege_RegionId",
                table: "Vorschlaege",
                column: "RegionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vorschlaege");
        }
    }
}
