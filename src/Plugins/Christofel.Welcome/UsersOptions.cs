//
//   UsersOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Welcome;

/// <summary>
/// Options with auth link.
/// </summary>
public class UsersOptions
{
    /// <summary>
    /// Gets or sets the auth link.
    /// </summary>
    public string AuthLink { get; set; } = string.Empty;
}