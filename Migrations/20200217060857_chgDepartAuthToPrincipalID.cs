using Microsoft.EntityFrameworkCore.Migrations;

namespace practice_mvc02.Migrations
{
    public partial class chgDepartAuthToPrincipalID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_departments_department_position_authority",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "authority",
                table: "departments");

            migrationBuilder.AddColumn<int>(
                name: "principalID",
                table: "departments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_departments_department_position_principalID",
                table: "departments",
                columns: new[] { "department", "position", "principalID" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_departments_department_position_principalID",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "principalID",
                table: "departments");

            migrationBuilder.AddColumn<int>(
                name: "authority",
                table: "departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_departments_department_position_authority",
                table: "departments",
                columns: new[] { "department", "position", "authority" },
                unique: true);
        }
    }
}
