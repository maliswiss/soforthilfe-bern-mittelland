using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Soforthilfe.Datenzugriff.Migrations
{
    /// <inheritdoc />
    public partial class Protokoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Protokoll",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RegionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Aktion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Objekt = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ObjektId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Detail = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Zeitpunkt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Protokoll", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Protokoll_Regionen_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regionen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Protokoll_RegionId_Zeitpunkt",
                table: "Protokoll",
                columns: new[] { "RegionId", "Zeitpunkt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Protokoll");
        }
    }
}
