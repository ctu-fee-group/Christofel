//
//   TemporalSlowmode.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Remora.Discord.Core;

namespace Christofel.Management.Database.Models
{
    /// <summary>
    ///     Representing table TemporalSlowmode holding data about slowmodes that were enabled by the bot
    /// </summary>
    public class TemporalSlowmode
    {
        /// <summary>
        ///     Primary key of <see cref="TemporalSlowmode" />>
        /// </summary>
        public int TemporalSlowmodeId { get; set; }

        /// <summary>
        ///     User who activated the slowmode
        /// </summary>
        public Snowflake UserId { get; set; }

        /// <summary>
        ///     Channel where the slowmode was enabled
        /// </summary>
        public Snowflake ChannelId { get; set; }

        /// <summary>
        ///     Interval between sending messages
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        ///     When should the slowmode be deactivated
        /// </summary>
        public DateTime DeactivationDate { get; set; }

        /// <summary>
        ///     When was the slowmode activated
        /// </summary>
        public DateTime ActivationDate { get; set; }
    }
}