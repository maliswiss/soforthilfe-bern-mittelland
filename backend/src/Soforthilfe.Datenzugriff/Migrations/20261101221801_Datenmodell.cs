using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Soforthilfe.Datenzugriff.Migrations
{
    /// <inheritdoc />
    public partial class Datenmodell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kategorien",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kennung = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Bezeichnung = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Kostenpflichtig = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategorien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regionen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kennung = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Aktiv = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regionen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KategorieId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kennung = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Bezeichnung = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Skills_Kategorien_KategorieId",
                        column: x => x.KategorieId,
                        principalTable: "Kategorien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Konten",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EMail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Anzeigename = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PasswortHash = table.Column<string>(type: "text", nullable: true),
                    Rolle = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Konten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Konten_Regionen_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regionen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Anfragen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionId = table.Column<Guid>(type: "uuid", nullable: false),
                    KontoId = table.Column<Guid>(type: "uuid", nullable: false),
                    KategorieId = table.Column<Guid>(type: "uuid", nullable: true),
                    Freitext = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Postleitzahl = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anfragen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anfragen_Kategorien_KategorieId",
                        column: x => x.KategorieId,
                        principalTable: "Kategorien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Anfragen_Konten_KontoId",
                        column: x => x.KontoId,
                        principalTable: "Konten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Anfragen_Regionen_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regionen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Helferprofile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KontoId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Postleitzahl = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Ort = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EinsatzradiusKm = table.Column<int>(type: "integer", nullable: false),
                    Einsatzbereit = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Helferprofile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Helferprofile_Konten_KontoId",
                        column: x => x.KontoId,
                        principalTable: "Konten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Helferprofile_Regionen_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regionen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HelferprofilSkills",
                columns: table => new
                {
                    HelferprofilId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelferprofilSkills", x => new { x.HelferprofilId, x.SkillsId });
                    table.ForeignKey(
                        name: "FK_HelferprofilSkills_Helferprofile_HelferprofilId",
                        column: x => x.HelferprofilId,
                        principalTable: "Helferprofile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HelferprofilSkills_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Kategorien",
                columns: new[] { "Id", "Bezeichnung", "Kennung", "Kostenpflichtig" },
                values: new object[,]
                {
                    { new Guid("01af187a-c8fa-47fa-a425-4b3b63c260a3"), "Begleitung", "begleitung", false },
                    { new Guid("426d6e5b-34b4-425c-b408-efa9a96d06bd"), "Handwerk und Reparaturen", "handwerk", true },
                    { new Guid("77a421f5-aa9f-4701-87b0-233dc23d7098"), "Computer und Smartphone", "digital", true },
                    { new Guid("79adfee0-8bc3-440f-abfd-3c0981d08d78"), "Orientierung und Formulare", "orientierung", false },
                    { new Guid("c87f69fb-01f1-4a32-9891-a7f31d9ffa20"), "Nachbarschaftshilfe", "nachbarschaft", false },
                    { new Guid("ff0714f3-a6bc-4578-8e1c-006043768441"), "Haushalt", "haushalt", true }
                });

            migrationBuilder.InsertData(
                table: "Regionen",
                columns: new[] { "Id", "Aktiv", "Kennung", "Name" },
                values: new object[,]
                {
                    { new Guid("9abd0c6c-da62-4469-99e0-cf9406a99ac6"), true, "bern", "Bern" },
                    { new Guid("a61871bc-f698-4e48-9d36-90fb837b56ae"), true, "mittelland", "Mittelland" }
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Bezeichnung", "KategorieId", "Kennung" },
                values: new object[,]
                {
                    { new Guid("403c1897-d38b-46a2-aaa5-118e15190285"), "Sanitär", new Guid("426d6e5b-34b4-425c-b408-efa9a96d06bd"), "sanitaer" },
                    { new Guid("47a61bb0-fae5-4202-9c46-8e6c12fb83c3"), "Reinigung", new Guid("ff0714f3-a6bc-4578-8e1c-006043768441"), "reinigung" },
                    { new Guid("6ae96d9a-d647-477c-8f50-7b91876b4c13"), "Haustiere betreuen", new Guid("c87f69fb-01f1-4a32-9891-a7f31d9ffa20"), "haustiere" },
                    { new Guid("82f5ec98-bdb2-4fc2-9228-2542d64a7fb1"), "Hilfe bei Formularen", new Guid("79adfee0-8bc3-440f-abfd-3c0981d08d78"), "formulare" },
                    { new Guid("8853ae85-82fe-4bf8-a46d-356e74bf282e"), "Computer und Smartphone einrichten", new Guid("77a421f5-aa9f-4701-87b0-233dc23d7098"), "computer" },
                    { new Guid("90385480-007c-47b5-9ae3-9e5e906b1fda"), "Einkauf", new Guid("c87f69fb-01f1-4a32-9891-a7f31d9ffa20"), "einkauf" },
                    { new Guid("a0e17f6f-a52d-4624-ad1c-050cad1c8e0c"), "Elektro", new Guid("426d6e5b-34b4-425c-b408-efa9a96d06bd"), "elektro" },
                    { new Guid("dab5a587-49b7-4a13-ba57-575e79aba354"), "Transport und Umzug", new Guid("ff0714f3-a6bc-4578-8e1c-006043768441"), "transport" },
                    { new Guid("dfdf01df-dd1c-4dfe-8138-810d32f1751f"), "Schlüsseldienst", new Guid("426d6e5b-34b4-425c-b408-efa9a96d06bd"), "schluesseldienst" },
                    { new Guid("f0828c46-e830-4fec-81e5-9006b4e00ddd"), "Begleitung zu Terminen", new Guid("01af187a-c8fa-47fa-a425-4b3b63c260a3"), "termin" },
                    { new Guid("f40abd8a-fd5a-473c-8064-362823f1b214"), "Spaziergang", new Guid("01af187a-c8fa-47fa-a425-4b3b63c260a3"), "spaziergang" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anfragen_KategorieId",
                table: "Anfragen",
                column: "KategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_Anfragen_KontoId",
                table: "Anfragen",
                column: "KontoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anfragen_RegionId",
                table: "Anfragen",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Helferprofile_KontoId",
                table: "Helferprofile",
                column: "KontoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Helferprofile_RegionId",
                table: "Helferprofile",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_HelferprofilSkills_SkillsId",
                table: "HelferprofilSkills",
                column: "SkillsId");

            migrationBuilder.CreateIndex(
                name: "IX_Kategorien_Kennung",
                table: "Kategorien",
                column: "Kennung",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Konten_EMail",
                table: "Konten",
                column: "EMail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Konten_RegionId",
                table: "Konten",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Regionen_Kennung",
                table: "Regionen",
                column: "Kennung",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_KategorieId",
                table: "Skills",
                column: "KategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Kennung",
                table: "Skills",
                column: "Kennung",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anfragen");

            migrationBuilder.DropTable(
                name: "HelferprofilSkills");

            migrationBuilder.DropTable(
                name: "Helferprofile");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Konten");

            migrationBuilder.DropTable(
                name: "Kategorien");

            migrationBuilder.DropTable(
                name: "Regionen");
        }
    }
}
