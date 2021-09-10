//
//   ContextualUserParser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// Parses <see cref="IUser"/> by using <see cref="ICommandContext"/>.
    /// </summary>
    /// <remarks>
    /// For interaction context, the data will be obtained from <see cref="IInteractionData.Resolved"/>,
    /// if it cannot be found there, error will be returned.
    ///
    /// For message context, the channel will be loaded using user api.
    ///
    /// Slash commands have to be executed
    /// with parameters using mentions, not by ids of the objects
    /// as that won't put them into Resolved.
    /// </remarks>
    public class ContextualUserParser : AbstractTypeParser<IUser>
    {
        private readonly ICommandContext? _commandContext;
        private readonly IDiscordRestUserAPI _userApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualUserParser"/> class.
        /// </summary>
        /// <param name="commandContext">The context of the command.</param>
        /// <param name="userApi">The user api.</param>
        public ContextualUserParser(IEnumerable<ICommandContext> commandContext, IDiscordRestUserAPI userApi)
        {
            _userApi = userApi;
            _commandContext = commandContext.FirstOrDefault();
        }

        /// <inheritdoc />
        public override ValueTask<Result<IUser>> TryParseAsync(string value, CancellationToken ct)
        {
            if (_commandContext is InteractionContext interactionContext &&
                Snowflake.TryParse(value.Unmention(), out var userID) &&
                interactionContext.Data.Resolved.IsDefined(out var resolved) &&
                resolved.Users.IsDefined(out var users) &&
                users.TryGetValue(userID.Value, out var user))
            {
                return ValueTask.FromResult(Result<IUser>.FromSuccess(user));
            }

            if (_commandContext is InteractionContext)
            {
                return ValueTask.FromResult<Result<IUser>>
                    (new ParsingError<IUser>("Could not find specified user in resolved data"));
            }

            return new UserParser(_userApi).TryParseAsync(value, ct);
        }
    }
}