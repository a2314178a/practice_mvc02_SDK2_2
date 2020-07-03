using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class workTimeRuleChgColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lateTime",
                table: "worktimerules");

            migrationBuilder.AddColumn<int>(
                name: "elasticityMin",
                table: "worktimerules",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 7, 3, 16, 57, 7, 900, DateTimeKind.Utc).AddTicks(4087), new DateTime(2020, 7, 3, 16, 57, 7, 900, DateTimeKind.Utc).AddTicks(4458) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "elasticityMin",
                table: "worktimerules");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "lateTime",
                table: "worktimerules",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 6, 24, 15, 16, 49, 532, DateTimeKind.Utc).AddTicks(2458), new DateTime(2020, 6, 24, 15, 16, 49, 532, DateTimeKind.Utc).AddTicks(2822) });
        }
    }
}
