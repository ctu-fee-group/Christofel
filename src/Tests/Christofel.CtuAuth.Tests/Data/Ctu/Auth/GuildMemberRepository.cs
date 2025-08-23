//
//   GuildMemberRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Christofel.Common.Database.Models;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth
{
    /// <summary>
    /// Repository for creating <see cref="IGuildMember"/>.
    /// </summary>
    public class GuildMemberRepository
    {
        private static readonly Random _rng = new Random();

        /// <summary>
        /// Creates guild member using the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to be set.</param>
        /// <param name="discordUsername">The username.</param>
        /// <returns>GuildMember representing the <paramref name="user"/>.</returns>
        public static GuildMember CreateDummyGuildMember(DbUser user, string discordUsername = "DummyUser") => new
            (
                new User(user.DiscordId, discordUsername, 0, default, default),
                default,
                default,
                default,
                new List<Snowflake>(),
                DateTimeOffset.Now,
                default,
                default,
                default,
                default
            );
    }
}
