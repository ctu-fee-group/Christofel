//
//   TeleportOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Enhancements.Teleport;

/// <summary>
/// Options for <see cref="TeleportCommandGroup"/>.
/// </summary>
public class TeleportOptions
{
    /// <summary>
    /// Gets or sets the message to be sent to the source channel.
    /// </summary>
    public string MessageFrom { get; set; } = "Teleport to {Channel} issued by {User}\n\n{Reference}";

    /// <summary>
    /// Gets or sets the message to be sent to the target channel.
    /// </summary>
    public string MessageTo { get; set; } = "Teleport from {Channel} issued by {User}\n\n{Reference}";
}