using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class AddUserUidCigar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserUid",
                table: "CigarsEF",
                type: "varchar(255)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserUid",
                table: "CigarsEF");
        }
    }
}
