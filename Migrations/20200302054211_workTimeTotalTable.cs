using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class workTimeTotalTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "worktimetotals",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    accountID = table.Column<int>(nullable: false),
                    dateMonth = table.Column<DateTime>(nullable: false),
                    totalTime = table.Column<double>(nullable: false),
                    createTime = table.Column<DateTime>(nullable: false),
                    updateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worktimetotals", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_worktimetotals_accountID_dateMonth",
                table: "worktimetotals",
                columns: new[] { "accountID", "dateMonth" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "worktimetotals");
        }
    }
}
