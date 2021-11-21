//
//   ContextualGuildMemberParser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Parsers;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.CommandsLib.ContextedParsers
{
    /// <summary>
    /// Parses <see cref="IPartialGuildMember"/> by using <see cref="ICommandContext"/>.
    /// </summary>
    /// <remarks>
    /// For interaction context, the data will be obtained from <see cref="IInteractionData.Resolved"/>,
    /// if it cannot be found there, error will be returned.
    ///
    /// For message context, the channel will be loaded using channel and guild api.
    ///
    /// Slash commands have to be executed
    /// with parameters using mentions, not by ids of the objects
    /// as that won't put them into Resolved.
    /// </remarks>
    public class ContextualGuildMemberParser : AbstractTypeParser<IPartialGuildMember>
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommandContext _commandContext;
        private readonly IDiscordRestGuildAPI _guildApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualGuildMemberParser"/> class.
        /// </summary>
        /// <param name="commandContext">The context of the command.</param>
        /// <param name="channelApi">The channel api.</param>
        /// <param name="guildApi">The guild api.</param>
        public ContextualGuildMemberParser
            (ICommandContext commandContext, IDiscordRestChannelAPI channelApi, IDiscordRestGuildAPI guildApi)
        {
            _guildApi = guildApi;
            _channelApi = channelApi;
            _commandContext = commandContext;
        }

        /// <inheritdoc />
        public override async ValueTask<Result<IPartialGuildMember>> TryParseAsync
            (string value, CancellationToken ct = default)
        {
            PartialGuildMember? retrievedMember = null;

            if (_commandContext is InteractionContext interactionContext &&
                Snowflake.TryParse(value.Unmention(), out var guildMemberID) &&
                interactionContext.Data.Resolved.IsDefined(out var resolved))
            {
                if (resolved.Members.IsDefined(out var members) &&
                    members.TryGetValue(guildMemberID.Value, out var member))
                {
                    retrievedMember = new PartialGuildMember
                    (
                        member.User,
                        member.Nickname,
                        member.Avatar,
                        member.Roles,
                        member.JoinedAt,
                        member.PremiumSince,
                        member.IsDeafened,
                        member.IsMuted,
                        member.IsPending,
                        member.Permissions
                    );
                }

                if (retrievedMember is not null &&
                    resolved.Users.IsDefined(out var users) &&
                    users.TryGetValue(guildMemberID.Value, out var user))
                {
                    retrievedMember = retrievedMember with { User = new Optional<IUser>(user) };
                }

                return retrievedMember;
            }

            if (_commandContext is InteractionContext)
            {
                return new ParsingError<IPartialGuildMember>("Could not find specified guild member in resolved data");
            }

            if (retrievedMember is null)
            {
                var parsed =
                    await new GuildMemberParser(_commandContext, _guildApi).TryParseAsync(value, ct);

                if (!parsed.IsSuccess)
                {
                    return Result<IPartialGuildMember>.FromError(parsed);
                }

                retrievedMember = new PartialGuildMember
                (
                    parsed.Entity.User,
                    parsed.Entity.Nickname,
                    parsed.Entity.Avatar,
                    new Optional<IReadOnlyList<Snowflake>>(parsed.Entity.Roles),
                    parsed.Entity.JoinedAt,
                    parsed.Entity.PremiumSince,
                    parsed.Entity.IsDeafened,
                    parsed.Entity.IsMuted,
                    parsed.Entity.IsPending,
                    parsed.Entity.Permissions
                );
            }

            return Result<IPartialGuildMember>.FromSuccess(retrievedMember);
        }
    }
}