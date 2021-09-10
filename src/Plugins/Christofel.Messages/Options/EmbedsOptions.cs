//
//   EmbedsOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.API.Objects;

namespace Christofel.Messages.Options
{
    /// <summary>
    /// Options for <see cref="EmbedProvider"/>.
    /// </summary>
    public class EmbedsOptions
    {
        /// <summary>
        /// Gets or sets folder where embeds should be loaded from.
        /// </summary>
        public string Folder { get; set; } = null!;
    }
}