//
//  CourseAssignment.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Remora.Rest.Core;

namespace Christofel.CoursesLib.Database;

/// <summary>
/// Database table that holds course to channel assignments.
/// </summary>
public class CourseAssignment
{
    /// <summary>
    /// Gets or sets the primary key of <see cref="CourseAssignment"/>.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the course in czech.
    /// </summary>
    public string CourseName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the key of the course.
    /// </summary>
    public string CourseKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the channel.
    /// </summary>
    public string? ChannelName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the key of the department of the course.
    /// </summary>
    public string DepartmentKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the assigned channel id.
    /// </summary>
    public Snowflake ChannelId { get; set; } = default;

    /// <summary>
    /// Gets or sets the role to assign the target channel with.
    /// </summary>
    public Snowflake? RoleId { get; set; } = default;

    /// <summary>
    /// Gets or sets the department foreign key resource.
    /// </summary>
    public DepartmentAssignment Department { get; set; } = null!;

    /// <summary>
    /// Gets or sets the group assignment resource.
    /// </summary>
    public CourseGroupAssignment GroupAssignment { get; set; } = null!;
}