//
//   GuildMemberRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Christofel.Common.Database.Models;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    /// <summary>
    /// Repository for creating <see cref="IGuildMember"/>.
    /// </summary>
    public class GuildMemberRepository
    {
        /// <summary>
        /// Creates guild member using the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to be set.</param>
        /// <returns>GuildMember representing the <paramref name="user"/>.</returns>
        public static GuildMember CreateDummyGuildMember(DbUser user) => new GuildMember
        (
            new User(user.DiscordId, "DummyUser", 1234, default, default),
            default,
            default,
            new List<Snowflake>(),
            default,
            default,
            default,
            default,
            default
        );
    }
}