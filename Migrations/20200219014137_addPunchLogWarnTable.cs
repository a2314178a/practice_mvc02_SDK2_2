using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class addPunchLogWarnTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "punchlogwarns",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    accountID = table.Column<int>(nullable: false),
                    principalID = table.Column<int>(nullable: false),
                    punchLogID = table.Column<int>(nullable: false),
                    warnStatus = table.Column<int>(nullable: false),
                    createTime = table.Column<DateTime>(nullable: false),
                    updateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_punchlogwarns", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_punchlogwarns_punchLogID",
                table: "punchlogwarns",
                column: "punchLogID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "punchlogwarns");
        }
    }
}
