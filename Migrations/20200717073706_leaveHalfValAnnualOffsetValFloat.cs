using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class leaveHalfValAnnualOffsetValFloat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<sbyte>(
                name: "halfVal",
                table: "leavenames",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<float>(
                name: "value",
                table: "annualdaysoffset",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 7, 17, 15, 37, 6, 360, DateTimeKind.Utc).AddTicks(3072), new DateTime(2020, 7, 17, 15, 37, 6, 360, DateTimeKind.Utc).AddTicks(3455) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "halfVal",
                table: "leavenames");

            migrationBuilder.AlterColumn<int>(
                name: "value",
                table: "annualdaysoffset",
                nullable: false,
                oldClrType: typeof(float));

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "createTime", "updateTime" },
                values: new object[] { new DateTime(2020, 7, 8, 15, 58, 39, 929, DateTimeKind.Utc).AddTicks(3186), new DateTime(2020, 7, 8, 15, 58, 39, 929, DateTimeKind.Utc).AddTicks(3558) });
        }
    }
}
