using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class whoCreateAccID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "whoCreateAccID",
                table: "employees",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "whoCreateAccID",
                table: "departments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "whoCreateAccID",
                table: "accounts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "whoCreateAccID",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "whoCreateAccID",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "whoCreateAccID",
                table: "accounts");
        }
    }
}
