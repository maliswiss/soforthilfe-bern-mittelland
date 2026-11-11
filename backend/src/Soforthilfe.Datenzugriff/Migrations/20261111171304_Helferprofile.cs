using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Soforthilfe.Datenzugriff.Migrations
{
    /// <inheritdoc />
    public partial class Helferprofile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Ort",
                table: "Helferprofile",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Adresse",
                table: "Helferprofile",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnzahlBewertungen",
                table: "Helferprofile",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AnzahlEinsaetze",
                table: "Helferprofile",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Beschreibung",
                table: "Helferprofile",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Betriebsname",
                table: "Helferprofile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Bewertungsschnitt",
                table: "Helferprofile",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "Partner",
                table: "Helferprofile",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Point>(
                name: "Standort",
                table: "Helferprofile",
                type: "geography (Point, 4326)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Typ",
                table: "Helferprofile",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Privatperson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adresse",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "AnzahlBewertungen",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "AnzahlEinsaetze",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "Beschreibung",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "Betriebsname",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "Bewertungsschnitt",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "Partner",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "Standort",
                table: "Helferprofile");

            migrationBuilder.DropColumn(
                name: "Typ",
                table: "Helferprofile");

            migrationBuilder.AlterColumn<string>(
                name: "Ort",
                table: "Helferprofile",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);
        }
    }
}
