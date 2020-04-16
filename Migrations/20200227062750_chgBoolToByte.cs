using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class chgBoolToByte : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "read",
                table: "msgsendreceive",
                type: "int(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "read",
                table: "msgsendreceive",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(1)");
        }
    }
}
