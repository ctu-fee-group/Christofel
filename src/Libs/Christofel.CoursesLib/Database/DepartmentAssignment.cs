//
//   DepartmentAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Christofel.CoursesLib.Database;

/// <summary>
/// Database table that holds department to channel assignments.
/// </summary>
[Table("DepartmentAssignments")]
public class DepartmentAssignment
{
    /// <summary>
    /// Gets or sets the primary key of <see cref="DepartmentAssignment"/>.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the key of the department.
    /// </summary>
    public string DepartmentKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the department.
    /// </summary>
    public string DepartmentName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the id of the assigned category.
    /// </summary>
    public Snowflake CategoryId { get; set; } = default;

    /// <summary>
    /// Gets or sets the courses for this department.
    /// </summary>
    public List<CourseAssignment>? Courses { get; set; } = default!;
}