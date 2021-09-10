//
//  20210806184152_MultileUserDuplicities.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace Christofel.BaseLib.Migrations
{
    /// <summary>
    /// Migration that allows multiple duplicates for a user.
    /// </summary>
    public partial class MultileUserDuplicities : Migration
    {
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
