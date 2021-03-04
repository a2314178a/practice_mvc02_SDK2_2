using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class newOvertime_TBof2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "overtimeApply",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    accountID = table.Column<int>(nullable: false),
                    note = table.Column<string>(nullable: true),
                    workDate = table.Column<DateTime>(nullable: false),
                    timeLength = table.Column<int>(nullable: false),
                    applyStatus = table.Column<int>(nullable: false),
                    lastOperaAccID = table.Column<int>(nullable: false),
                    createTime = table.Column<DateTime>(nullable: false),
                    updateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_overtimeApply", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "overTimeRest",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    accountID = table.Column<int>(nullable: false),
                    canRestTime = table.Column<int>(nullable: false),
                    lastOperaAccID = table.Column<int>(nullable: false),
                    createTime = table.Column<DateTime>(nullable: false),
                    updateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_overTimeRest", x => x.ID);
                });

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2021, 3, 3, 15, 25, 36, 597, DateTimeKind.Utc).AddTicks(2209), new DateTime(2021, 3, 3, 15, 25, 36, 597, DateTimeKind.Utc).AddTicks(2482) });

            migrationBuilder.CreateIndex(
                name: "IX_overTimeRest_accountID",
                table: "overTimeRest",
                column: "accountID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "overtimeApply");

            migrationBuilder.DropTable(
                name: "overTimeRest");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 7, 17, 15, 37, 6, 360, DateTimeKind.Utc).AddTicks(3072), new DateTime(2020, 7, 17, 15, 37, 6, 360, DateTimeKind.Utc).AddTicks(3455) });
        }
    }
}
