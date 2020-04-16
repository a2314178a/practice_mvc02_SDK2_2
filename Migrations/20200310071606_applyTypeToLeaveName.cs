using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class applyTypeToLeaveName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "applyType",
                table: "leaveofficeapplys");

            migrationBuilder.AddColumn<int>(
                name: "leaveID",
                table: "leaveofficeapplys",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "leaveID",
                table: "leaveofficeapplys");

            migrationBuilder.AddColumn<int>(
                name: "applyType",
                table: "leaveofficeapplys",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
