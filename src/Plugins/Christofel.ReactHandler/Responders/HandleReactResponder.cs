//
//   HandleReactResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Christofel.Common.Database;
using Christofel.Helpers.Helpers;
using Christofel.ReactHandler.Database;
using Christofel.ReactHandler.Database.Models;
using Christofel.ReactHandler.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.ReactHandler.Responders
{
    /// <summary>
    /// Responder that handles reactions on marked messages.
    /// </summary>
    /// <remarks>
    /// Assigns channels or roles to users who react to marked messages.
    /// </remarks>
    public class HandleReactResponder
        : IResponder<IMessageReactionRemove>, IResponder<IMessageReactionAdd>
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IReadableDbContext _dbContext;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleReactResponder"/> class.
        /// </summary>
        /// <param name="dbContext">The react handler database context.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="guildApi">The guild api.</param>
        /// <param name="channelApi">The channel api.</param>
        public HandleReactResponder
        (
            IReadableDbContext<ReactHandlerContext> dbContext,
            ILogger<HandleReactResponder> logger,
            IDiscordRestGuildAPI guildApi,
            IDiscordRestChannelAPI channelApi
        )
        {
            _channelApi = channelApi;
            _guildApi = guildApi;
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<Result> RespondAsync
        (
            IMessageReactionAdd gatewayEvent,
            CancellationToken ct = default
        )
        {
            if (!gatewayEvent.GuildID.IsDefined(out var guildId))
            {
                return Result.FromSuccess();
            }

            if (IsBot(gatewayEvent.Member))
            {
                return Result.FromSuccess();
            }

            string emoji = EmojiFormatter.GetEmojiString(gatewayEvent.Emoji);
            var matchingHandlers = await _dbContext.Set<HandleReact>()
                .Where
                (
                    x => x.ChannelId == gatewayEvent.ChannelID &&
                        x.MessageId == gatewayEvent.MessageID &&
                        x.Emoji == emoji
                )
                .ToListAsync(ct);

            List<IResult> errors = new List<IResult>();
            foreach (var matchingHandler in matchingHandlers)
            {
                if (matchingHandler.Emoji != emoji)
                {
                    continue;
                }

                Result result;

                switch (matchingHandler.Type)
                {
                    case HandleReactType.Channel:
                        result = await AssignChannel
                        (
                            guildId,
                            gatewayEvent.UserID,
                            matchingHandler.EntityId,
                            ct
                        );
                        break;
                    case HandleReactType.Role:
                        result = await AssignRole
                        (
                            guildId,
                            gatewayEvent.UserID,
                            matchingHandler.EntityId,
                            ct
                        );
                        break;
                    default:
                        return new InvalidOperationError("Unknown matching handler type");
                }

                if (!result.IsSuccess)
                {
                    _logger.LogResultError
                    (
                        result,
                        $"Could not assign channel or role ({matchingHandler.EntityId}) to user ({gatewayEvent.UserID})."
                    );
                    errors.Add(result);
                }
                else
                {
                    _logger.LogInformation
                    (
                        "Assigned {ChannelOrRole} to user <@{User}>",
                        HandleReactFormatter.FormatHandlerTarget(matchingHandler),
                        gatewayEvent.UserID
                    );
                }
            }

            if (errors.Count > 0)
            {
                return new AggregateError(errors, "Could not assign some channels or roles to the user.");
            }

            return Result.FromSuccess();
        }

        // TODO: somehow merge respond methods to one?

        /// <inheritdoc />
        public async Task<Result> RespondAsync
        (
            IMessageReactionRemove gatewayEvent,
            CancellationToken ct = default
        )
        {
            if (!gatewayEvent.GuildID.IsDefined(out var guildId))
            {
                return Result.FromSuccess();
            }

            string emoji = EmojiFormatter.GetEmojiString(gatewayEvent.Emoji);
            var matchingHandlers = await _dbContext.Set<HandleReact>()
                .Where
                (
                    x => x.ChannelId == gatewayEvent.ChannelID &&
                        x.MessageId == gatewayEvent.MessageID &&
                        x.Emoji == emoji
                )
                .ToListAsync(ct);

            List<IResult> errors = new List<IResult>();
            foreach (var matchingHandler in matchingHandlers)
            {
                Result result;

                if (matchingHandler.Emoji != emoji)
                {
                    continue;
                }

                switch (matchingHandler.Type)
                {
                    case HandleReactType.Channel:
                        result = await DeassignChannel
                        (
                            guildId,
                            gatewayEvent.UserID,
                            matchingHandler.EntityId,
                            ct
                        );
                        break;
                    case HandleReactType.Role:
                        result = await DeassignRole
                        (
                            guildId,
                            gatewayEvent.UserID,
                            matchingHandler.EntityId,
                            ct
                        );
                        break;
                    default:
                        return new InvalidOperationError("Unknown matching handler type");
                }

                if (!result.IsSuccess)
                {
                    _logger.LogResultError
                        (result, $"Could not deassign channel or role ({matchingHandler.EntityId}) from user.");
                    errors.Add(result);
                }
                else
                {
                    _logger.LogInformation
                    (
                        "Deassigned {ChannelOrRole} from user <@{User}>",
                        HandleReactFormatter.FormatHandlerTarget(matchingHandler),
                        gatewayEvent.UserID
                    );
                }
            }

            if (errors.Count > 0)
            {
                return new AggregateError(errors, "Could not deassign some channels or roles to the user.");
            }

            return Result.FromSuccess();
        }

        private Task<Result> AssignRole
        (
            Snowflake guildId,
            Snowflake userId,
            Snowflake roleId,
            CancellationToken ct
        )
            => _guildApi.AddGuildMemberRoleAsync
            (
                guildId,
                userId,
                roleId,
                "Reaction handler",
                ct
            );

        private Task<Result> AssignChannel
        (
            Snowflake guildId,
            Snowflake userId,
            Snowflake channelId,
            CancellationToken ct
        )
            => _channelApi.EditChannelPermissionsAsync
            (
                channelId,
                userId,
                new DiscordPermissionSet(DiscordPermission.ViewChannel),
                type: PermissionOverwriteType.Member,
                reason: "Reaction handler",
                ct: ct
            );

        private Task<Result> DeassignRole
        (
            Snowflake guildId,
            Snowflake userId,
            Snowflake roleId,
            CancellationToken ct
        )
            => _guildApi.RemoveGuildMemberRoleAsync
            (
                guildId,
                userId,
                roleId,
                "Reaction handler",
                ct
            );

        private Task<Result> DeassignChannel
        (
            Snowflake guildId,
            Snowflake userId,
            Snowflake channelId,
            CancellationToken ct
        )
            => _channelApi.EditChannelPermissionsAsync
            (
                channelId,
                userId,
                deny: new DiscordPermissionSet(DiscordPermission.ViewChannel),
                type: PermissionOverwriteType.Member,
                reason: "Reaction handler",
                ct: ct
            );

        private bool IsBot(Optional<IGuildMember> optMember)
        {
            if (optMember.IsDefined(out var member))
            {
                if (member.User.IsDefined(out var user))
                {
                    if (user.IsBot.IsDefined(out var bot))
                    {
                        return bot;
                    }
                }
            }

            return false;
        }
    }
}