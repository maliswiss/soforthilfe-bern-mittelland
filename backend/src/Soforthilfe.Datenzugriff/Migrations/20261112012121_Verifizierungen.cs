using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Soforthilfe.Datenzugriff.Migrations
{
    /// <inheritdoc />
    public partial class Verifizierungen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Verifizierung",
                table: "Helferprofile",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unverifiziert");

            migrationBuilder.CreateTable(
                name: "Verifizierungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionId = table.Column<Guid>(type: "uuid", nullable: false),
                    HelferprofilId = table.Column<Guid>(type: "uuid", nullable: false),
                    Art = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nachweiswert = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Bemerkung = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GeprueftVonId = table.Column<Guid>(type: "uuid", nullable: true),
                    EingereichtAm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GeprueftAm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verifizierungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Verifizierungen_Helferprofile_HelferprofilId",
                        column: x => x.HelferprofilId,
                        principalTable: "Helferprofile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Verifizierungen_Regionen_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regionen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Verifizierungen_HelferprofilId",
                table: "Verifizierungen",
                column: "HelferprofilId");

            migrationBuilder.CreateIndex(
                name: "IX_Verifizierungen_RegionId_Status_EingereichtAm",
                table: "Verifizierungen",
                columns: new[] { "RegionId", "Status", "EingereichtAm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Verifizierungen");

            migrationBuilder.DropColumn(
                name: "Verifizierung",
                table: "Helferprofile");
        }
    }
}
