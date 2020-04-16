using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class addLeaveOfficeApply : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "leaveofficeapplys",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    accountID = table.Column<int>(nullable: false),
                    principalID = table.Column<int>(nullable: false),
                    applyType = table.Column<int>(nullable: false),
                    optionType = table.Column<int>(nullable: false),
                    note = table.Column<string>(nullable: true),
                    startTime = table.Column<DateTime>(nullable: false),
                    endTime = table.Column<DateTime>(nullable: false),
                    applyStatus = table.Column<int>(nullable: false),
                    lastOperaAccID = table.Column<int>(nullable: false),
                    createTime = table.Column<DateTime>(nullable: false),
                    updateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leaveofficeapplys", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_punchcardlogs_accountID_logDate",
                table: "punchcardlogs",
                columns: new[] { "accountID", "logDate" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "leaveofficeapplys");

            migrationBuilder.DropIndex(
                name: "IX_punchcardlogs_accountID_logDate",
                table: "punchcardlogs");
        }
    }
}
