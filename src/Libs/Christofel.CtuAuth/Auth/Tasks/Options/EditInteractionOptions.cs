//
//  EditInteractionOptions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.CtuAuth.Auth.Tasks.Options;

/// <summary>
/// Options for <see cref="EditInteractionResponseTask"/>.
/// </summary>
public class EditInteractionOptions
{
    /// <summary>
    /// Gets or sets the new message.
    /// </summary>
    public string EditedMessage { get; set; } = string.Empty;
}