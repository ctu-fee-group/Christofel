//
//  20221006194247_CoursesGroupSetNameOptional.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Christofel.CoursesLib.Migrations
{
    public partial class CoursesGroupSetNameOptional : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_CourseGroupAssignment_ChannelId",
                schema: "Courses",
                table: "CourseGroupAssignments");

            migrationBuilder.RenameIndex(
                name: "IX_CourseGroupAssignment_ChannelId",
                schema: "Courses",
                table: "CourseGroupAssignments",
                newName: "IX_CourseGroupAssignments_ChannelId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Courses",
                table: "CourseGroupAssignments",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_CourseGroupAssignments_ChannelId",
                schema: "Courses",
                table: "CourseGroupAssignments",
                column: "ChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAssignments_CourseGroupAssignments_ChannelId",
                schema: "Courses",
                table: "CourseAssignments",
                column: "ChannelId",
                principalSchema: "Courses",
                principalTable: "CourseGroupAssignments",
                principalColumn: "ChannelId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_CourseGroupAssignments_ChannelId",
                schema: "Courses",
                table: "CourseGroupAssignments");

            migrationBuilder.RenameIndex(
                name: "IX_CourseGroupAssignments_ChannelId",
                schema: "Courses",
                table: "CourseGroupAssignments",
                newName: "IX_CourseGroupAssignment_ChannelId");

            migrationBuilder.UpdateData(
                schema: "Courses",
                table: "CourseGroupAssignments",
                keyColumn: "Name",
                keyValue: null,
                column: "Name",
                value: string.Empty);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Courses",
                table: "CourseGroupAssignments",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_CourseGroupAssignment_ChannelId",
                schema: "Courses",
                table: "CourseGroupAssignments",
                column: "ChannelId");
        }
    }
}
