using Microsoft.EntityFrameworkCore.Migrations;

namespace Christofel.BaseLib.Migrations
{
    public partial class MakeUserDuplicityOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_DuplicitUserId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "DuplicitUserId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_DuplicitUserId",
                table: "Users",
                column: "DuplicitUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_DuplicitUserId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "DuplicitUserId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_DuplicitUserId",
                table: "Users",
                column: "DuplicitUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
