//
//   AutoPinOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Rest.Core;

namespace Christofel.Enhancements.AutoPin;

/// <summary>
/// Options for <see cref="AutoPinResponder"/>.
/// </summary>
public class AutoPinOptions
{
    /// <summary>
    /// Gets or sets the emojis to count.
    /// </summary>
    public string[] AutoPinEmojis { get; set; } = { "📌" };

    /// <summary>
    /// Gets or sets the default minimum count of the emojis to pin the message.
    /// </summary>
    /// <remarks>
    /// Use zero to disable.
    /// </remarks>
    public short MinimumCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the minimum count of emojis to pin the message.
    /// </summary>
    /// <remarks>
    /// Use zero to disable in the given channel/category.
    /// </remarks>
    public Dictionary<Snowflake, short>? MinimumCountOverrides { get; set; } = null;
}