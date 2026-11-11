using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Soforthilfe.Datenzugriff.Migrations
{
    /// <inheritdoc />
    public partial class Preisangaben : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AnfahrtChf",
                table: "Helferprofile",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MwstPflichtig",
                table: "Helferprofile",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NachtzuschlagProzent",
                table: "Helferprofile",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StundenansatzChf",
                table: "Helferprofile",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WochenendzuschlagProzent",
                table: "Helferprofile",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnfahrtChf",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "MwstPflichtig",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "NachtzuschlagProzent",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "StundenansatzChf",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "WochenendzuschlagProzent",
                table: "Helferprofile");
        }
    }
}
