using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class emAnnualLastOpIDcreateOpLogTB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "lastOperaAccID",
                table: "employeeannualleaves",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "operateLogs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    operateID = table.Column<int>(nullable: false),
                    employeeID = table.Column<int>(nullable: false),
                    active = table.Column<string>(nullable: true),
                    category = table.Column<string>(nullable: true),
                    content = table.Column<string>(nullable: true),
                    remark = table.Column<string>(nullable: true),
                    createTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operateLogs", x => x.ID);
                });

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 6, 9, 13, 35, 10, 393, DateTimeKind.Utc).AddTicks(1154), new DateTime(2020, 6, 9, 13, 35, 10, 393, DateTimeKind.Utc).AddTicks(1538) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operateLogs");

            migrationBuilder.DropColumn(
                name: "lastOperaAccID",
                table: "employeeannualleaves");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 5, 29, 15, 17, 29, 99, DateTimeKind.Utc).AddTicks(3602), new DateTime(2020, 5, 29, 15, 17, 29, 99, DateTimeKind.Utc).AddTicks(3973) });
        }
    }
}
