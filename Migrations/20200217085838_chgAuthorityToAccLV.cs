using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class chgAuthorityToAccLV : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "authority",
                table: "accounts");

            migrationBuilder.AddColumn<int>(
                name: "accLV",
                table: "accounts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "accLV",
                table: "accounts");

            migrationBuilder.AddColumn<int>(
                name: "authority",
                table: "accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
