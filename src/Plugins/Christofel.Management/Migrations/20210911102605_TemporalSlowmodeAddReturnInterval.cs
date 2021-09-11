//
//  20210911102605_TemporalSlowmodeAddReturnInterval.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Christofel.Management.Migrations
{
    /// <summary>
    /// Migration that adds ReturnInterval property to TemporalSlowmode.
    /// </summary>
    public partial class TemporalSlowmodeAddReturnInterval : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "ReturnInterval",
                table: "TemporalSlowmodes",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnInterval",
                table: "TemporalSlowmodes");
        }
    }
}
