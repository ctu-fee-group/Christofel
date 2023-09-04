//
//  UserEmailCode.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace Christofel.Common.Database.Models;

/// <summary>
/// The email code for a user trying to authenticate using an e-mail.
/// </summary>
public class UserEmailCode
{
    /// <summary>
    /// The id of the user email code.
    /// </summary>
    [Key]
    public int UserEmailId { get; set; }

    /// <summary>
    /// Gets or sets the id of the user associated with this record.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user associated with this record.
    /// </summary>
    public DbUser User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the code in the email.
    /// </summary>
    [MaxLength(50)]
    public string Code { get; set; } = null!;
}