//
//   DiscordTarget.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Database.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Core;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Represents discord user or role.
    /// </summary>
    /// <remarks>
    /// TargetType.Everyone is for representing every user and role.
    /// </remarks>
    [Owned]
    public class DiscordTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordTarget"/> class.
        /// </summary>
        /// <param name="discordId">Id of the discord entity.</param>
        /// <param name="type">Type of the target.</param>
        public DiscordTarget(Snowflake discordId, TargetType type)
        {
            DiscordId = discordId;
            TargetType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordTarget"/> class.
        /// </summary>
        /// <param name="discordId">Id of the discord entity.</param>
        /// <param name="type">Type of the target.</param>
        public DiscordTarget(ulong discordId, TargetType type)
            : this(new Snowflake(discordId), type)
        {
            DiscordId = new Snowflake(discordId);
            TargetType = type;
        }

        /// <summary>
        /// Gets <see cref="DiscordTarget"/> representing .
        /// </summary>
        public static DiscordTarget Everyone => new DiscordTarget(0, TargetType.Everyone);

        /// <summary>
        /// Gets or sets the id of the discord entity.
        /// </summary>
        public Snowflake DiscordId { get; set; }

        /// <summary>
        /// Gets or sets the guild id the entity is inside.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        public TargetType TargetType { get; set; }
    }
}