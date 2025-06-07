//
//  20221006185143_AddCoursesGroup.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Christofel.CoursesLib.Migrations
{
    public partial class AddCoursesGroup : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DepartmentKey",
                schema: "Courses",
                table: "CourseAssignments",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CourseKey",
                schema: "Courses",
                table: "CourseAssignments",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ChannelName",
                schema: "Courses",
                table: "CourseAssignments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentKey",
                schema: "Courses",
                table: "DepartmentAssignments",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_DepartmentAssignment_DepartmentKey",
                schema: "Courses",
                table: "DepartmentAssignments",
                column: "DepartmentKey");

            migrationBuilder.CreateTable(
                name: "CourseGroupAssignments",
                schema: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChannelId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseGroupAssignments", x => x.Id);
                    table.UniqueConstraint("AK_CourseGroupAssignments_ChannelId", x => x.ChannelId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAssignments_ChannelId",
                schema: "Courses",
                table: "CourseAssignments",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAssignments_CourseKey",
                schema: "Courses",
                table: "CourseAssignments",
                column: "CourseKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseAssignments_DepartmentKey",
                schema: "Courses",
                table: "CourseAssignments",
                column: "DepartmentKey");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAssignment_DepartmentKey",
                schema: "Courses",
                table: "DepartmentAssignments",
                column: "DepartmentKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseGroupAssignments_ChannelId",
                schema: "Courses",
                table: "CourseGroupAssignments",
                column: "ChannelId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAssignments_DepartmentAssignment_DepartmentKey",
                schema: "Courses",
                table: "CourseAssignments",
                column: "DepartmentKey",
                principalSchema: "Courses",
                principalTable: "DepartmentAssignments",
                principalColumn: "DepartmentKey",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseAssignments_DepartmentAssignment_DepartmentKey",
                schema: "Courses",
                table: "CourseAssignments");

            migrationBuilder.DropTable(
                name: "CourseGroupAssignments",
                schema: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_CourseAssignments_ChannelId",
                schema: "Courses",
                table: "CourseAssignments");

            migrationBuilder.DropIndex(
                name: "IX_CourseAssignments_CourseKey",
                schema: "Courses",
                table: "CourseAssignments");

            migrationBuilder.DropIndex(
                name: "IX_CourseAssignments_DepartmentKey",
                schema: "Courses",
                table: "CourseAssignments");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_DepartmentAssignment_DepartmentKey",
                schema: "Courses",
                table: "DepartmentAssignments");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentAssignment_DepartmentKey",
                schema: "Courses",
                table: "DepartmentAssignments");

            migrationBuilder.DropColumn(
                name: "ChannelName",
                schema: "Courses",
                table: "CourseAssignments");

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentKey",
                schema: "Courses",
                table: "CourseAssignments",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CourseKey",
                schema: "Courses",
                table: "CourseAssignments",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentKey",
                schema: "Courses",
                table: "DepartmentAssignments",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
