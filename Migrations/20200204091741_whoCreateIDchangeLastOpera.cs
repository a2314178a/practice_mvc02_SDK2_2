using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class whoCreateIDchangeLastOpera : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "lastOperaAccID",
                table: "employees",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "lastOperaAccID",
                table: "departments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "lastOperaAccID",
                table: "accounts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lastOperaAccID",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "lastOperaAccID",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "lastOperaAccID",
                table: "accounts");

            migrationBuilder.AddColumn<int>(
                name: "whoCreateAccID",
                table: "employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "whoCreateAccID",
                table: "departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "whoCreateAccID",
                table: "accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
