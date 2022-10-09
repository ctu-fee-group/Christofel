//
//  20221009195128_AddCourseUsersLink.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Christofel.CoursesLib.Migrations
{
    public partial class AddCourseUsersLink : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_CourseAssignments_CourseKey",
                schema: "Courses",
                table: "CourseAssignments",
                column: "CourseKey");

            migrationBuilder.CreateTable(
                name: "CourseUsers",
                schema: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CourseKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserDiscordId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseUsers_CourseAssignments_CourseKey",
                        column: x => x.CourseKey,
                        principalSchema: "Courses",
                        principalTable: "CourseAssignments",
                        principalColumn: "CourseKey",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CourseUsers_CourseKey",
                schema: "Courses",
                table: "CourseUsers",
                column: "CourseKey");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseUsers",
                schema: "Courses");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_CourseAssignments_CourseKey",
                schema: "Courses",
                table: "CourseAssignments");
        }
    }
}
