//
//   UsernameRoleAssignment.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Christofel.Common.Database.Models;

/// <summary>
/// Database table for assignment of roles based on the username of the person.
/// It is expected this table will be filled from an external source, where
/// querying the source during authentication is too expensive / undesirabllee.
/// </summary>
[Table("UsernameRoleAssignment", Schema = ChristofelBaseContext.SchemaName)]
public class UsernameRoleAssignment
{
    /// <summary>
    /// Gets of sets primary key of the <see cref="UsernameRoleAssignment"/>.
    /// </summary>
    [Key]
    public int UsernameRoleAssignmentId { get; set; }

    /// <summary>
    /// Gets or sets the username to match against to assign the role to.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Gets of sets id of the assignment.
    /// </summary>
    public int AssignmentId { get; set; }

    /// <summary>
    /// Gets or sets the assignment.
    /// </summary>
    public RoleAssignment Assignment { get; set; } = null!;
}
