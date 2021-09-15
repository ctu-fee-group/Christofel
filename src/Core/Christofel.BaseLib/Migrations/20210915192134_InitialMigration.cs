//
//  20210915192134_InitialMigration.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Christofel.BaseLib.Migrations
{
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Core");

            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PermissionAssignment",
                schema: "Core",
                columns: table => new
                {
                    PermissionAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PermissionName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target_DiscordId = table.Column<long>(type: "bigint", nullable: false),
                    Target_GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Target_TargetType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionAssignment", x => x.PermissionAssignmentId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleAssignment",
                schema: "Core",
                columns: table => new
                {
                    RoleAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    RoleType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAssignment", x => x.RoleAssignmentId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "User",
                schema: "Core",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AuthenticatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CtuUsername = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DuplicityApproved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DuplicitUserId = table.Column<int>(type: "int", nullable: true),
                    RegistrationCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiscordId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_User_User_DuplicitUserId",
                        column: x => x.DuplicitUserId,
                        principalSchema: "Core",
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProgrammeRoleAssignment",
                schema: "Core",
                columns: table => new
                {
                    ProgrammeRoleAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Programme = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgrammeRoleAssignment", x => x.ProgrammeRoleAssignmentId);
                    table.ForeignKey(
                        name: "FK_ProgrammeRoleAssignment_RoleAssignment_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "Core",
                        principalTable: "RoleAssignment",
                        principalColumn: "RoleAssignmentId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpecificRoleAssignment",
                schema: "Core",
                columns: table => new
                {
                    SpecificRoleAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecificRoleAssignment", x => x.SpecificRoleAssignmentId);
                    table.ForeignKey(
                        name: "FK_SpecificRoleAssignment_RoleAssignment_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "Core",
                        principalTable: "RoleAssignment",
                        principalColumn: "RoleAssignmentId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TitleRoleAssignment",
                schema: "Core",
                columns: table => new
                {
                    TitleRoleAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Post = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Pre = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<uint>(type: "int unsigned", nullable: false),
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleRoleAssignment", x => x.TitleRoleAssignmentId);
                    table.ForeignKey(
                        name: "FK_TitleRoleAssignment_RoleAssignment_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "Core",
                        principalTable: "RoleAssignment",
                        principalColumn: "RoleAssignmentId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UsermapRoleAssignment",
                schema: "Core",
                columns: table => new
                {
                    UsermapRoleAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UsermapRole = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegexMatch = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsermapRoleAssignment", x => x.UsermapRoleAssignmentId);
                    table.ForeignKey(
                        name: "FK_UsermapRoleAssignment_RoleAssignment_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "Core",
                        principalTable: "RoleAssignment",
                        principalColumn: "RoleAssignmentId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YearRoleAssignment",
                schema: "Core",
                columns: table => new
                {
                    YearRoleAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Year = table.Column<int>(type: "int", nullable: false),
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearRoleAssignment", x => x.YearRoleAssignmentId);
                    table.ForeignKey(
                        name: "FK_YearRoleAssignment_RoleAssignment_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "Core",
                        principalTable: "RoleAssignment",
                        principalColumn: "RoleAssignmentId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProgrammeRoleAssignment_AssignmentId",
                schema: "Core",
                table: "ProgrammeRoleAssignment",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificRoleAssignment_AssignmentId",
                schema: "Core",
                table: "SpecificRoleAssignment",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TitleRoleAssignment_AssignmentId",
                schema: "Core",
                table: "TitleRoleAssignment",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_User_DuplicitUserId",
                schema: "Core",
                table: "User",
                column: "DuplicitUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsermapRoleAssignment_AssignmentId",
                schema: "Core",
                table: "UsermapRoleAssignment",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_YearRoleAssignment_AssignmentId",
                schema: "Core",
                table: "YearRoleAssignment",
                column: "AssignmentId");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionAssignment",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "ProgrammeRoleAssignment",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "SpecificRoleAssignment",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "TitleRoleAssignment",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "User",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "UsermapRoleAssignment",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "YearRoleAssignment",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "RoleAssignment",
                schema: "Core");
        }
    }
}
