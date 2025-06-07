//
//  CourseUser.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using Christofel.Common.Database.Models;
using Remora.Rest.Core;

namespace Christofel.CoursesLib.Database;

/// <summary>
/// Assigns <see cref="CourseAssignment"/> to <see cref="DbUser"/>.
/// </summary>
public class CourseUser
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the key of the course.
    /// </summary>
    public string CourseKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user discord id.
    /// </summary>
    public Snowflake UserDiscordId { get; set; }

    /// <summary>
    /// Gets or sets the course assignment.
    /// </summary>
    public CourseAssignment Course { get; set; } = null!;
}