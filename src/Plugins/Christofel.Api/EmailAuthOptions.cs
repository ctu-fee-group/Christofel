//
//  EmailAuthOptions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.Api;

/// <summary>
/// Options for email authentication.
/// </summary>
public class EmailAuthOptions
{
    /// <summary>
    /// Gets or sets the roles to assign when authenticating using an e-mail.
    /// </summary>
    public List<ulong> AssignRoles { get; set; } = null!;
}