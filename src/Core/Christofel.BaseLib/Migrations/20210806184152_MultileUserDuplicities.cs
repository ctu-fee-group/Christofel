using Microsoft.EntityFrameworkCore.Migrations;

namespace Christofel.BaseLib.Migrations
{
    public partial class MultileUserDuplicities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("set FOREIGN_KEY_CHECKS=0;");
            
            migrationBuilder.DropIndex(
                name: "IX_Users_DuplicitUserId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DuplicitUserId",
                table: "Users",
                column: "DuplicitUserId");
            
            migrationBuilder.Sql("set FOREIGN_KEY_CHECKS=1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("set FOREIGN_KEY_CHECKS=0;");
            
            migrationBuilder.DropIndex(
                name: "IX_Users_DuplicitUserId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DuplicitUserId",
                table: "Users",
                column: "DuplicitUserId",
                unique: true);
            
            migrationBuilder.Sql("set FOREIGN_KEY_CHECKS=1;");
        }
    }
}
