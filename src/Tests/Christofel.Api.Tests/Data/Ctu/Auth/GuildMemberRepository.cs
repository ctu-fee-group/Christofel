using System.Collections.Generic;
using Christofel.BaseLib.Database.Models;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    public class GuildMemberRepository
    {
        public static GuildMember CreateDummyGuildMember(DbUser user)
        {
            return new GuildMember(new User(user.DiscordId, "DummyUser", 1234, default), default, new List<Snowflake>(),
                default, default, default, default);
        }
    }
}