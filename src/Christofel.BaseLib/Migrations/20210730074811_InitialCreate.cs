using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Christofel.BaseLib.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PermissionName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Target_DiscordId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Target_GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Target_TargetType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionAssignmentId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleAssignments",
                columns: table => new
                {
                    RoleAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RoleType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAssignments", x => x.RoleAssignmentId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AuthenticatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DiscordId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CtuUsername = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Duplicity = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DuplicityApproved = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProgrammeRoleAssignments",
                columns: table => new
                {
                    ProgrammeRoleAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Programme = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgrammeRoleAssignments", x => x.ProgrammeRoleAssignmentId);
                    table.ForeignKey(
                        name: "FK_ProgrammeRoleAssignments_RoleAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "RoleAssignments",
                        principalColumn: "RoleAssignmentId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TitleRoleAssignment",
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
                        name: "FK_TitleRoleAssignment_RoleAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "RoleAssignments",
                        principalColumn: "RoleAssignmentId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UsermapRoleAssignments",
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
                    table.PrimaryKey("PK_UsermapRoleAssignments", x => x.UsermapRoleAssignmentId);
                    table.ForeignKey(
                        name: "FK_UsermapRoleAssignments_RoleAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "RoleAssignments",
                        principalColumn: "RoleAssignmentId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "YearRoleAssignments",
                columns: table => new
                {
                    YearRoleAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Year = table.Column<int>(type: "int", nullable: false),
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearRoleAssignments", x => x.YearRoleAssignmentId);
                    table.ForeignKey(
                        name: "FK_YearRoleAssignments_RoleAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "RoleAssignments",
                        principalColumn: "RoleAssignmentId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProgrammeRoleAssignments_AssignmentId",
                table: "ProgrammeRoleAssignments",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TitleRoleAssignment_AssignmentId",
                table: "TitleRoleAssignment",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UsermapRoleAssignments_AssignmentId",
                table: "UsermapRoleAssignments",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_YearRoleAssignments_AssignmentId",
                table: "YearRoleAssignments",
                column: "AssignmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "ProgrammeRoleAssignments");

            migrationBuilder.DropTable(
                name: "TitleRoleAssignment");

            migrationBuilder.DropTable(
                name: "UsermapRoleAssignments");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "YearRoleAssignments");

            migrationBuilder.DropTable(
                name: "RoleAssignments");
        }
    }
}
