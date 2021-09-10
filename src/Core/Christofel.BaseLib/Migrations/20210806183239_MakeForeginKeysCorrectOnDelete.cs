//
//  20210806183239_MakeForeginKeysCorrectOnDelete.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace Christofel.BaseLib.Migrations
{
    /// <summary>
    /// Migration that makes foreign keys behave correctly on delete.
    /// </summary>
    public partial class MakeForeginKeysCorrectOnDelete : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgrammeRoleAssignments_RoleAssignments_AssignmentId",
                table: "ProgrammeRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TitleRoleAssignment_RoleAssignments_AssignmentId",
                table: "TitleRoleAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_UsermapRoleAssignments_RoleAssignments_AssignmentId",
                table: "UsermapRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_DuplicitUserId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_YearRoleAssignments_RoleAssignments_AssignmentId",
                table: "YearRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Users_DuplicitUserId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DuplicitUserId",
                table: "Users",
                column: "DuplicitUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgrammeRoleAssignments_RoleAssignments_AssignmentId",
                table: "ProgrammeRoleAssignments",
                column: "AssignmentId",
                principalTable: "RoleAssignments",
                principalColumn: "RoleAssignmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TitleRoleAssignment_RoleAssignments_AssignmentId",
                table: "TitleRoleAssignment",
                column: "AssignmentId",
                principalTable: "RoleAssignments",
                principalColumn: "RoleAssignmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UsermapRoleAssignments_RoleAssignments_AssignmentId",
                table: "UsermapRoleAssignments",
                column: "AssignmentId",
                principalTable: "RoleAssignments",
                principalColumn: "RoleAssignmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_DuplicitUserId",
                table: "Users",
                column: "DuplicitUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_YearRoleAssignments_RoleAssignments_AssignmentId",
                table: "YearRoleAssignments",
                column: "AssignmentId",
                principalTable: "RoleAssignments",
                principalColumn: "RoleAssignmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgrammeRoleAssignments_RoleAssignments_AssignmentId",
                table: "ProgrammeRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TitleRoleAssignment_RoleAssignments_AssignmentId",
                table: "TitleRoleAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_UsermapRoleAssignments_RoleAssignments_AssignmentId",
                table: "UsermapRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_DuplicitUserId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_YearRoleAssignments_RoleAssignments_AssignmentId",
                table: "YearRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Users_DuplicitUserId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DuplicitUserId",
                table: "Users",
                column: "DuplicitUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgrammeRoleAssignments_RoleAssignments_AssignmentId",
                table: "ProgrammeRoleAssignments",
                column: "AssignmentId",
                principalTable: "RoleAssignments",
                principalColumn: "RoleAssignmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TitleRoleAssignment_RoleAssignments_AssignmentId",
                table: "TitleRoleAssignment",
                column: "AssignmentId",
                principalTable: "RoleAssignments",
                principalColumn: "RoleAssignmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsermapRoleAssignments_RoleAssignments_AssignmentId",
                table: "UsermapRoleAssignments",
                column: "AssignmentId",
                principalTable: "RoleAssignments",
                principalColumn: "RoleAssignmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_DuplicitUserId",
                table: "Users",
                column: "DuplicitUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YearRoleAssignments_RoleAssignments_AssignmentId",
                table: "YearRoleAssignments",
                column: "AssignmentId",
                principalTable: "RoleAssignments",
                principalColumn: "RoleAssignmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
