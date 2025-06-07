//
//  20230821171301_AddCourseChannelRoleId.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Christofel.CoursesLib.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseChannelRoleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RoleId",
                schema: "Courses",
                table: "CourseAssignments",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleId",
                schema: "Courses",
                table: "CourseAssignments");
        }
    }
}
