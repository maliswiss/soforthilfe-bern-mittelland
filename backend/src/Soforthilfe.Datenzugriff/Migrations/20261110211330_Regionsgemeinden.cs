using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Soforthilfe.Datenzugriff.Migrations
{
    /// <inheritdoc />
    public partial class Regionsgemeinden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "Gemeinden",
                table: "Regionen",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.UpdateData(
                table: "Regionen",
                keyColumn: "Id",
                keyValue: new Guid("9abd0c6c-da62-4469-99e0-cf9406a99ac6"),
                column: "Gemeinden",
                value: new[] { 307, 351, 352, 353, 354, 355, 356, 357, 358, 359, 360, 361, 362, 363, 403, 535, 538, 540, 541, 543, 544, 546, 551, 553, 557, 602, 603, 605, 606, 607, 608, 609, 610, 611, 612, 613, 614, 615, 616, 617, 619, 620, 622, 623, 626, 627, 628, 629, 630, 632, 662, 663, 665, 666, 667, 668, 669, 670, 671, 852, 853, 855, 861, 866, 868, 869, 870, 872, 877, 879, 880, 884, 888, 889 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gemeinden",
                table: "Regionen");
        }
    }
}
