//
//   CourseGroupAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using Remora.Rest.Core;

namespace Christofel.CoursesLib.Database;

/// <summary>
/// Custom named group of multiple courses for given channel.
/// </summary>
public class CourseGroupAssignment
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the id of the channel linked to the group.
    /// </summary>
    public Snowflake ChannelId { get; set; } = default;

    /// <summary>
    /// Gets or sets the custom name of the group.
    /// </summary>
    public string? Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the courses assigned to this group.
    /// </summary>
    public List<CourseAssignment>? Courses { get; set; }
}