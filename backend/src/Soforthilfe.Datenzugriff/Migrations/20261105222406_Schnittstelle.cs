using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Soforthilfe.Datenzugriff.Migrations
{
    /// <inheritdoc />
    public partial class Schnittstelle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Regionen",
                keyColumn: "Id",
                keyValue: new Guid("a61871bc-f698-4e48-9d36-90fb837b56ae"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("403c1897-d38b-46a2-aaa5-118e15190285"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("47a61bb0-fae5-4202-9c46-8e6c12fb83c3"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("6ae96d9a-d647-477c-8f50-7b91876b4c13"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("82f5ec98-bdb2-4fc2-9228-2542d64a7fb1"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("8853ae85-82fe-4bf8-a46d-356e74bf282e"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("90385480-007c-47b5-9ae3-9e5e906b1fda"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("a0e17f6f-a52d-4624-ad1c-050cad1c8e0c"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("dab5a587-49b7-4a13-ba57-575e79aba354"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("dfdf01df-dd1c-4dfe-8138-810d32f1751f"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("f0828c46-e830-4fec-81e5-9006b4e00ddd"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("f40abd8a-fd5a-473c-8064-362823f1b214"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("426d6e5b-34b4-425c-b408-efa9a96d06bd"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("77a421f5-aa9f-4701-87b0-233dc23d7098"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("79adfee0-8bc3-440f-abfd-3c0981d08d78"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("ff0714f3-a6bc-4578-8e1c-006043768441"));

            migrationBuilder.AddColumn<bool>(
                name: "NurOrientierung",
                table: "Kategorien",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Reihenfolge",
                table: "Kategorien",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Adresse",
                table: "Anfragen",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ort",
                table: "Anfragen",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Prioritaet",
                table: "Anfragen",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Zeitfenster",
                table: "Anfragen",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("01af187a-c8fa-47fa-a425-4b3b63c260a3"),
                columns: new[] { "Bezeichnung", "NurOrientierung", "Reihenfolge" },
                values: new object[] { "Begleitung und Orientierung", true, 10 });

            migrationBuilder.UpdateData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("c87f69fb-01f1-4a32-9891-a7f31d9ffa20"),
                columns: new[] { "Kennung", "NurOrientierung", "Reihenfolge" },
                values: new object[] { "nachbarschaftshilfe", false, 9 });

            migrationBuilder.InsertData(
                table: "Kategorien",
                columns: new[] { "Id", "Bezeichnung", "Kennung", "Kostenpflichtig", "NurOrientierung", "Reihenfolge" },
                values: new object[,]
                {
                    { new Guid("3000be64-c436-4751-ac80-e3d4b20b50bd"), "Lieferung Apotheke und Einkauf", "lieferdienst", true, false, 8 },
                    { new Guid("3e09dd9e-7619-4b11-9aee-b0dc7150aacf"), "Kleine Reparatur", "kleine-reparatur", true, false, 2 },
                    { new Guid("495056ab-d82d-4e33-be3b-dc3e96e6b75a"), "Schlüsseldienst", "schluesseldienst", true, false, 1 },
                    { new Guid("7013d888-2f47-4ea8-9736-b91825f849d3"), "IT- und Handy-Soforthilfe", "it-soforthilfe", true, false, 6 },
                    { new Guid("725c18b0-04e0-4f12-8c08-2ac5c2ec9d64"), "Panne und Abschleppdienst", "pannenhilfe", true, false, 4 },
                    { new Guid("8d71da98-bf81-45df-9633-d5b3d2d0b6a7"), "Sanitär, Heizung, Elektro", "sanitaer-elektro", true, false, 3 },
                    { new Guid("aba279c9-09f5-4426-b30a-2b04b6baa08a"), "Reinigung nach Schäden", "reinigung", true, false, 7 },
                    { new Guid("e3b9eb6e-e4b7-47d2-997b-63d4ad05e262"), "Transport- und Umzugshilfe", "transporthilfe", true, false, 5 }
                });

            migrationBuilder.UpdateData(
                table: "Regionen",
                keyColumn: "Id",
                keyValue: new Guid("9abd0c6c-da62-4469-99e0-cf9406a99ac6"),
                columns: new[] { "Kennung", "Name" },
                values: new object[] { "bern-mittelland", "Soforthilfe Bern/Mittelland" });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Bezeichnung", "KategorieId", "Kennung" },
                values: new object[,]
                {
                    { new Guid("11c55505-ba55-464c-86ea-f34088f5ab36"), "Nachbarschaftshilfe", new Guid("c87f69fb-01f1-4a32-9891-a7f31d9ffa20"), "nachbarschaftshilfe" },
                    { new Guid("137f3e2b-d5fe-4c8e-814d-1941b47121d4"), "Begleitung und Orientierung", new Guid("01af187a-c8fa-47fa-a425-4b3b63c260a3"), "begleitung" },
                    { new Guid("086f4f0d-06ad-4856-9d05-0c5136cb3f93"), "Reinigung nach Schäden", new Guid("aba279c9-09f5-4426-b30a-2b04b6baa08a"), "reinigung" },
                    { new Guid("45cc60eb-cce3-41c6-afa0-92b265177884"), "Transport- und Umzugshilfe", new Guid("e3b9eb6e-e4b7-47d2-997b-63d4ad05e262"), "transporthilfe" },
                    { new Guid("4cc55036-09e9-454e-a2fb-9d4a7ea4bd43"), "Lieferdienst Apotheke und Einkauf", new Guid("3000be64-c436-4751-ac80-e3d4b20b50bd"), "lieferdienst" },
                    { new Guid("7d0fc8b8-f5e2-44f3-84e3-0c83452a7b33"), "IT- und Handy-Soforthilfe", new Guid("7013d888-2f47-4ea8-9736-b91825f849d3"), "it-soforthilfe" },
                    { new Guid("a68e8a08-980e-4fc6-86b1-0baf5da9172e"), "Kleine Reparatur", new Guid("3e09dd9e-7619-4b11-9aee-b0dc7150aacf"), "kleine-reparatur" },
                    { new Guid("c9a7b813-8f26-4dc1-8d0f-28d4a3cbad74"), "Sanitär, Heizung, Elektro", new Guid("8d71da98-bf81-45df-9633-d5b3d2d0b6a7"), "sanitaer-elektro" },
                    { new Guid("f38abfd7-19c8-45b1-9343-f5cf66a40b4b"), "Pannen- und Abschleppdienst", new Guid("725c18b0-04e0-4f12-8c08-2ac5c2ec9d64"), "pannenhilfe" },
                    { new Guid("f944be5a-3e77-471b-a0e0-97d82287547c"), "Schlüsseldienst", new Guid("495056ab-d82d-4e33-be3b-dc3e96e6b75a"), "schluesseldienst" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("086f4f0d-06ad-4856-9d05-0c5136cb3f93"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("11c55505-ba55-464c-86ea-f34088f5ab36"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("137f3e2b-d5fe-4c8e-814d-1941b47121d4"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("45cc60eb-cce3-41c6-afa0-92b265177884"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("4cc55036-09e9-454e-a2fb-9d4a7ea4bd43"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("7d0fc8b8-f5e2-44f3-84e3-0c83452a7b33"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("a68e8a08-980e-4fc6-86b1-0baf5da9172e"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("c9a7b813-8f26-4dc1-8d0f-28d4a3cbad74"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("f38abfd7-19c8-45b1-9343-f5cf66a40b4b"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("f944be5a-3e77-471b-a0e0-97d82287547c"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("3000be64-c436-4751-ac80-e3d4b20b50bd"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("3e09dd9e-7619-4b11-9aee-b0dc7150aacf"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("495056ab-d82d-4e33-be3b-dc3e96e6b75a"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("7013d888-2f47-4ea8-9736-b91825f849d3"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("725c18b0-04e0-4f12-8c08-2ac5c2ec9d64"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("8d71da98-bf81-45df-9633-d5b3d2d0b6a7"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("aba279c9-09f5-4426-b30a-2b04b6baa08a"));

            migrationBuilder.DeleteData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("e3b9eb6e-e4b7-47d2-997b-63d4ad05e262"));

            migrationBuilder.DropColumn(
                name: "NurOrientierung",
                table: "Kategorien");

            migrationBuilder.DropColumn(
                name: "Reihenfolge",
                table: "Kategorien");

            migrationBuilder.DropColumn(
                name: "Adresse",
                table: "Anfragen");

            migrationBuilder.DropColumn(
                name: "Ort",
                table: "Anfragen");

            migrationBuilder.DropColumn(
                name: "Prioritaet",
                table: "Anfragen");

            migrationBuilder.DropColumn(
                name: "Zeitfenster",
                table: "Anfragen");

            migrationBuilder.UpdateData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("01af187a-c8fa-47fa-a425-4b3b63c260a3"),
                column: "Bezeichnung",
                value: "Begleitung");

            migrationBuilder.UpdateData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: new Guid("c87f69fb-01f1-4a32-9891-a7f31d9ffa20"),
                column: "Kennung",
                value: "nachbarschaft");

            migrationBuilder.InsertData(
                table: "Kategorien",
                columns: new[] { "Id", "Bezeichnung", "Kennung", "Kostenpflichtig" },
                values: new object[,]
                {
                    { new Guid("426d6e5b-34b4-425c-b408-efa9a96d06bd"), "Handwerk und Reparaturen", "handwerk", true },
                    { new Guid("77a421f5-aa9f-4701-87b0-233dc23d7098"), "Computer und Smartphone", "digital", true },
                    { new Guid("79adfee0-8bc3-440f-abfd-3c0981d08d78"), "Orientierung und Formulare", "orientierung", false },
                    { new Guid("ff0714f3-a6bc-4578-8e1c-006043768441"), "Haushalt", "haushalt", true }
                });

            migrationBuilder.UpdateData(
                table: "Regionen",
                keyColumn: "Id",
                keyValue: new Guid("9abd0c6c-da62-4469-99e0-cf9406a99ac6"),
                columns: new[] { "Kennung", "Name" },
                values: new object[] { "bern", "Bern" });

            migrationBuilder.InsertData(
                table: "Regionen",
                columns: new[] { "Id", "Aktiv", "Kennung", "Name" },
                values: new object[] { new Guid("a61871bc-f698-4e48-9d36-90fb837b56ae"), true, "mittelland", "Mittelland" });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Bezeichnung", "KategorieId", "Kennung" },
                values: new object[,]
                {
                    { new Guid("6ae96d9a-d647-477c-8f50-7b91876b4c13"), "Haustiere betreuen", new Guid("c87f69fb-01f1-4a32-9891-a7f31d9ffa20"), "haustiere" },
                    { new Guid("90385480-007c-47b5-9ae3-9e5e906b1fda"), "Einkauf", new Guid("c87f69fb-01f1-4a32-9891-a7f31d9ffa20"), "einkauf" },
                    { new Guid("f0828c46-e830-4fec-81e5-9006b4e00ddd"), "Begleitung zu Terminen", new Guid("01af187a-c8fa-47fa-a425-4b3b63c260a3"), "termin" },
                    { new Guid("f40abd8a-fd5a-473c-8064-362823f1b214"), "Spaziergang", new Guid("01af187a-c8fa-47fa-a425-4b3b63c260a3"), "spaziergang" },
                    { new Guid("403c1897-d38b-46a2-aaa5-118e15190285"), "Sanitär", new Guid("426d6e5b-34b4-425c-b408-efa9a96d06bd"), "sanitaer" },
                    { new Guid("47a61bb0-fae5-4202-9c46-8e6c12fb83c3"), "Reinigung", new Guid("ff0714f3-a6bc-4578-8e1c-006043768441"), "reinigung" },
                    { new Guid("82f5ec98-bdb2-4fc2-9228-2542d64a7fb1"), "Hilfe bei Formularen", new Guid("79adfee0-8bc3-440f-abfd-3c0981d08d78"), "formulare" },
                    { new Guid("8853ae85-82fe-4bf8-a46d-356e74bf282e"), "Computer und Smartphone einrichten", new Guid("77a421f5-aa9f-4701-87b0-233dc23d7098"), "computer" },
                    { new Guid("a0e17f6f-a52d-4624-ad1c-050cad1c8e0c"), "Elektro", new Guid("426d6e5b-34b4-425c-b408-efa9a96d06bd"), "elektro" },
                    { new Guid("dab5a587-49b7-4a13-ba57-575e79aba354"), "Transport und Umzug", new Guid("ff0714f3-a6bc-4578-8e1c-006043768441"), "transport" },
                    { new Guid("dfdf01df-dd1c-4dfe-8138-810d32f1751f"), "Schlüsseldienst", new Guid("426d6e5b-34b4-425c-b408-efa9a96d06bd"), "schluesseldienst" }
                });
        }
    }
}
