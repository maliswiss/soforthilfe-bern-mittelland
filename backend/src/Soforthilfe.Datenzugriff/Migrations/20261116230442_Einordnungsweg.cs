using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Soforthilfe.Datenzugriff.Migrations
{
    /// <inheritdoc />
    public partial class Einordnungsweg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "EinordnungKonfidenz",
                table: "Anfragen",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Erfassungsweg",
                table: "Anfragen",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EinordnungKonfidenz",
                table: "Anfragen");

            migrationBuilder.DropColumn(
                name: "Erfassungsweg",
                table: "Anfragen");
        }
    }
}
