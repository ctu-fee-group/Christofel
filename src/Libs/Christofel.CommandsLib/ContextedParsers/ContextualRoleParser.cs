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
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.CommandsLib.ContextedParsers
{
    /// <summary>
    /// Parses <see cref="IRole"/> by using <see cref="ICommandContext"/>.
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
    public class ContextualRoleParser : AbstractTypeParser<IRole>
    {
        private readonly ICommandContext _commandContext;
        private readonly IDiscordRestGuildAPI _guildApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualRoleParser"/> class.
        /// </summary>
        /// <param name="commandContext">The context of the command.</param>
        /// <param name="guildApi">The guild api.</param>
        public ContextualRoleParser
            (ICommandContext commandContext, IDiscordRestGuildAPI guildApi)
        {
            _guildApi = guildApi;
            _commandContext = commandContext;
        }

        /// <inheritdoc />
        public override ValueTask<Result<IRole>> TryParseAsync(string value, CancellationToken ct)
        {
            if (_commandContext is InteractionContext interactionContext &&
                Snowflake.TryParse(value.Unmention(), out var roleID) &&
                interactionContext.Interaction.Data.TryGet(out var interactionData) &&
                interactionData.TryPickT0(out var data, out _) &&
                data.Resolved.IsDefined(out var resolved) &&
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

            return new RoleParser(_commandContext, _guildApi).TryParseAsync(value, ct);
        }
    }
}