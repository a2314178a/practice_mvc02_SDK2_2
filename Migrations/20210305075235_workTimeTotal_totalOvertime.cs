using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class workTimeTotal_totalOvertime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "totalOvertime",
                table: "worktimetotals",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2021, 3, 5, 15, 52, 35, 488, DateTimeKind.Utc).AddTicks(9565), new DateTime(2021, 3, 5, 15, 52, 35, 488, DateTimeKind.Utc).AddTicks(9861) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "totalOvertime",
                table: "worktimetotals");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2021, 3, 3, 15, 25, 36, 597, DateTimeKind.Utc).AddTicks(2209), new DateTime(2021, 3, 3, 15, 25, 36, 597, DateTimeKind.Utc).AddTicks(2482) });
        }
    }
}
