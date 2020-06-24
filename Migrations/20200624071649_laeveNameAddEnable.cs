using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class laeveNameAddEnable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<sbyte>(
                name: "enable",
                table: "leavenames",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 6, 24, 15, 16, 49, 532, DateTimeKind.Utc).AddTicks(2458), new DateTime(2020, 6, 24, 15, 16, 49, 532, DateTimeKind.Utc).AddTicks(2822) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "enable",
                table: "leavenames");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 6, 9, 13, 35, 10, 393, DateTimeKind.Utc).AddTicks(1154), new DateTime(2020, 6, 9, 13, 35, 10, 393, DateTimeKind.Utc).AddTicks(1538) });
        }
    }
}
