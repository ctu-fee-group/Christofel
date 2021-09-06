using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Implementations.ReadOnlyDatabase;
using Christofel.ReactHandler.Database;
using Christofel.ReactHandler.Database.Models;
using Christofel.ReactHandler.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Christofel.ReactHandler.Responders
{
    public class HandleReactResponder
        : IResponder<IMessageReactionRemove>, IResponder<IMessageReactionAdd>
    {
        private readonly IReadableDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly IDiscordRestChannelAPI _channelApi;

        public HandleReactResponder(ReadOnlyDbContext<ReactHandlerContext> dbContext,
            ILogger<HandleReactResponder> logger, IDiscordRestGuildAPI guildApi, IDiscordRestChannelAPI channelApi)
        {
            _channelApi = channelApi;
            _guildApi = guildApi;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<Result> RespondAsync(IMessageReactionAdd gatewayEvent,
            CancellationToken ct = new CancellationToken())
        {
            if (!gatewayEvent.GuildID.IsDefined(out var guildId))
            {
                return Result.FromSuccess();
            }
            
            string emoji = EmojiFormatter.GetEmojiString(gatewayEvent.Emoji);
            var matchingHandlers = await _dbContext.Set<HandleReact>()
                .Where(x => x.ChannelId == gatewayEvent.ChannelID && x.MessageId == gatewayEvent.MessageID &&
                            x.Emoji == emoji)
                .ToListAsync(ct);
            
            List<IResultError> errors = new List<IResultError>();
            foreach (var matchingHandler in matchingHandlers)
            {
                Result result;

                switch (matchingHandler.Type)
                {
                    case HandleReactType.Channel:
                        result = await AssignChannel(guildId, gatewayEvent.UserID,
                            matchingHandler.EntityId, ct);
                        break;
                    case HandleReactType.Role:
                        result = await AssignRole(guildId, gatewayEvent.UserID, matchingHandler.EntityId,
                            ct);
                        break;
                    default:
                        return new InvalidOperationError("Unknown matching handler type");
                }

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Could not assign channel or role ({ChannelOrRole}) to user. {Error}",
                        matchingHandler.EntityId, result.Error.Message);
                    errors.Add(result.Error);
                }
                else
                {
                    _logger.LogInformation("Assigned {ChannelOrRole} to user <@{User}>",
                        HandleReactFormatter.FormatHandlerTarget(matchingHandler), gatewayEvent.UserID);
                }
            }

            if (errors.Count > 0)
            {
                return new AggregateError(errors, "Could not assign some channels or roles to the user.");
            }

            return Result.FromSuccess();
        }

        // TODO: somehow merge respond methods to one?
        public async Task<Result> RespondAsync(IMessageReactionRemove gatewayEvent,
            CancellationToken ct = new CancellationToken())
        {
            if (!gatewayEvent.GuildID.IsDefined(out var guildId))
            {
                return Result.FromSuccess();
            }

            string emoji = EmojiFormatter.GetEmojiString(gatewayEvent.Emoji);
            var matchingHandlers = await _dbContext.Set<HandleReact>()
                .Where(x => x.ChannelId == gatewayEvent.ChannelID && x.MessageId == gatewayEvent.MessageID &&
                            x.Emoji == emoji)
                .ToListAsync(ct);
            
            List<IResultError> errors = new List<IResultError>();
            foreach (var matchingHandler in matchingHandlers)
            {
                Result result;

                switch (matchingHandler.Type)
                {
                    case HandleReactType.Channel:
                        result = await DeassignChannel(guildId, gatewayEvent.UserID,
                            matchingHandler.EntityId, ct);
                        break;
                    case HandleReactType.Role:
                        result = await DeassignRole(guildId, gatewayEvent.UserID, matchingHandler.EntityId,
                            ct);
                        break;
                    default:
                        return new InvalidOperationError("Unknown matching handler type");
                }

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Could not deassign channel or role ({ChannelOrRole}) from user. {Error}",
                        matchingHandler.EntityId, result.Error.Message);
                    errors.Add(result.Error);
                }
                else
                {
                    _logger.LogInformation("Deassigned {ChannelOrRole} from user <@{User}>",
                        HandleReactFormatter.FormatHandlerTarget(matchingHandler), gatewayEvent.UserID);
                }
            }

            if (errors.Count > 0)
            {
                return new AggregateError(errors, "Could not deassign some channels or roles to the user.");
            }

            return Result.FromSuccess();
        }

        private Task<Result> AssignRole(Snowflake guildId, Snowflake userId, Snowflake roleId, CancellationToken ct)
        {
            return _guildApi.AddGuildMemberRoleAsync(guildId, userId, roleId, "Reaction handler", ct);
        }

        private Task<Result> AssignChannel(Snowflake guildId, Snowflake userId, Snowflake channelId,
            CancellationToken ct)
        {
            return _channelApi.EditChannelPermissionsAsync(channelId, userId,
                allow: new DiscordPermissionSet(DiscordPermission.ViewChannel), type: PermissionOverwriteType.Member,
                reason: "Reaction handler", ct: ct);
        }

        private Task<Result> DeassignRole(Snowflake guildId, Snowflake userId, Snowflake roleId, CancellationToken ct)
        {
            return _guildApi.RemoveGuildMemberRoleAsync(guildId, userId, roleId, "Reaction handler", ct);
        }

        private Task<Result> DeassignChannel(Snowflake guildId, Snowflake userId, Snowflake channelId,
            CancellationToken ct)
        {
            return _channelApi.EditChannelPermissionsAsync(channelId, userId,
                deny: new DiscordPermissionSet(DiscordPermission.ViewChannel), type: PermissionOverwriteType.Member,
                reason: "Reaction handler", ct: ct);
        }
    }
}