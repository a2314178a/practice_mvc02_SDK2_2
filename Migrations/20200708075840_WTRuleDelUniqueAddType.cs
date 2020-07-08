using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class WTRuleDelUniqueAddType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_worktimerules_startTime_endTime",
                table: "worktimerules");

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "worktimerules",
                type: "int(1)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 7, 8, 15, 58, 39, 929, DateTimeKind.Utc).AddTicks(3186), new DateTime(2020, 7, 8, 15, 58, 39, 929, DateTimeKind.Utc).AddTicks(3558) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "worktimerules");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 7, 3, 16, 57, 7, 900, DateTimeKind.Utc).AddTicks(4087), new DateTime(2020, 7, 3, 16, 57, 7, 900, DateTimeKind.Utc).AddTicks(4458) });

            migrationBuilder.CreateIndex(
                name: "IX_worktimerules_startTime_endTime",
                table: "worktimerules",
                columns: new[] { "startTime", "endTime" },
                unique: true);
        }
    }
}
