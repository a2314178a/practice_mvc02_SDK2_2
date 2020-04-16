using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class uniqueRuleIDGroupTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_worktimerule",
                table: "worktimerule");

            migrationBuilder.RenameTable(
                name: "worktimerule",
                newName: "worktimerules");

            migrationBuilder.AddColumn<int>(
                name: "timeRuleID",
                table: "accounts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_worktimerules",
                table: "worktimerules",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "grouprules",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    groupName = table.Column<string>(nullable: true),
                    ruleParameter = table.Column<int>(nullable: false),
                    lastOperaAccID = table.Column<int>(nullable: false),
                    createTime = table.Column<DateTime>(nullable: false),
                    updateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grouprules", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_worktimerules_startTime_endTime",
                table: "worktimerules",
                columns: new[] { "startTime", "endTime" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grouprules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_worktimerules",
                table: "worktimerules");

            migrationBuilder.DropIndex(
                name: "IX_worktimerules_startTime_endTime",
                table: "worktimerules");

            migrationBuilder.DropColumn(
                name: "timeRuleID",
                table: "accounts");

            migrationBuilder.RenameTable(
                name: "worktimerules",
                newName: "worktimerule");

            migrationBuilder.AddPrimaryKey(
                name: "PK_worktimerule",
                table: "worktimerule",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    accountID = table.Column<int>(type: "int", nullable: false),
                    createTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    departmentID = table.Column<int>(type: "int", nullable: false),
                    lastOperaAccID = table.Column<int>(type: "int", nullable: false),
                    updateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.ID);
                });
        }
    }
}
