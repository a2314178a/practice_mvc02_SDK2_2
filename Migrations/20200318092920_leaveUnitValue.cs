using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class leaveUnitValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "unit",
                table: "leaveofficeapplys",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<float>(
                name: "unitVal",
                table: "leaveofficeapplys",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unit",
                table: "leaveofficeapplys");

            migrationBuilder.DropColumn(
                name: "unitVal",
                table: "leaveofficeapplys");
        }
    }
}
