//
//   UsersOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Management;

/// <summary>
/// Options for users management.
/// </summary>
public class UsersOptions
{
    /// <summary>
    /// Gets or sets the authentication link.
    /// </summary>
    /// <remarks>
    /// Should contain {code} that will be replaced by the registration code.
    /// </remarks>
    public string AuthLink { get; set; } = string.Empty;
}