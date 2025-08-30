//
//   AuthOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.CtuAuth.Auth.Tasks.Options;

/// <summary>
/// Options for authentication flow.
/// </summary>
public class AuthOptions
{
    /// <summary>
    /// Gets or sets code of the primary faculty.
    /// </summary>
    /// <remarks>
    /// This is currently used for assignign year roles,
    /// those are assigned only to students of this faculty.
    /// </remarks>
    public string? FacultyCode { get; set; }
}
