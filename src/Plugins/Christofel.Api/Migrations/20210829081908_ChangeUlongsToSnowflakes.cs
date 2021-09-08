using Microsoft.EntityFrameworkCore.Migrations;

namespace Christofel.Api.Migrations
{
    public partial class ChangeUlongsToSnowflakes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "UserDiscordId",
                table: "AssignRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");

            migrationBuilder.AlterColumn<long>(
                name: "RoleId",
                table: "AssignRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");

            migrationBuilder.AlterColumn<long>(
                name: "GuildDiscordId",
                table: "AssignRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "UserDiscordId",
                table: "AssignRoles",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "RoleId",
                table: "AssignRoles",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildDiscordId",
                table: "AssignRoles",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
