using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class emDetailAddSome : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "agentEnable",
                table: "employeedetails",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "myAgentID",
                table: "employeedetails",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sex",
                table: "employeedetails",
                type: "int(1)",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "agentEnable",
                table: "employeedetails");

            migrationBuilder.DropColumn(
                name: "myAgentID",
                table: "employeedetails");

            migrationBuilder.DropColumn(
                name: "sex",
                table: "employeedetails");
        }
    }
}
