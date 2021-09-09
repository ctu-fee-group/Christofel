//
//   ContextualRoleParser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Parsers;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.CommandsLib.ContextedParsers
{
    public class ContextualRoleParser : AbstractTypeParser<IRole>
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommandContext _commandContext;
        private readonly IDiscordRestGuildAPI _guildApi;

        public ContextualRoleParser
            (ICommandContext commandContext, IDiscordRestChannelAPI channelApi, IDiscordRestGuildAPI guildApi)
        {
            _channelApi = channelApi;
            _guildApi = guildApi;
            _commandContext = commandContext;
        }

        public override ValueTask<Result<IRole>> TryParseAsync(string value, CancellationToken ct)
        {
            if (_commandContext is InteractionContext interactionContext &&
                Snowflake.TryParse(value.Unmention(), out var roleID) &&
                interactionContext.Data.Resolved.IsDefined(out var resolved) &&
                resolved.Roles.IsDefined(out var roles) &&
                roles.TryGetValue(roleID.Value, out var role))
            {
                return ValueTask.FromResult(Result<IRole>.FromSuccess(role));
            }

            if (_commandContext is InteractionContext)
            {
                return new ValueTask<Result<IRole>>
                    (new ParsingError<IRole>("Could not find specified role in resolved data"));
            }

            return new RoleParser(_commandContext, _channelApi, _guildApi).TryParseAsync(value, ct);
        }
    }
}