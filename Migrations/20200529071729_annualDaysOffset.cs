using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class annualDaysOffset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "annualdaysoffset",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    emAnnualID = table.Column<int>(nullable: false),
                    reason = table.Column<string>(nullable: true),
                    value = table.Column<int>(nullable: false),
                    lastOperaAccID = table.Column<int>(nullable: false),
                    createTime = table.Column<DateTime>(nullable: false),
                    updateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annualdaysoffset", x => x.ID);
                });

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 5, 29, 15, 17, 29, 99, DateTimeKind.Utc).AddTicks(3602), new DateTime(2020, 5, 29, 15, 17, 29, 99, DateTimeKind.Utc).AddTicks(3973) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "annualdaysoffset");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 5, 20, 16, 2, 0, 700, DateTimeKind.Utc).AddTicks(1644), new DateTime(2020, 5, 20, 16, 2, 0, 700, DateTimeKind.Utc).AddTicks(2007) });
        }
    }
}
