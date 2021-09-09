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
    public class ContextualUserParser : AbstractTypeParser<IUser>
    {
        private readonly ICommandContext? _commandContext;
        private readonly IDiscordRestUserAPI _userApi;

        public ContextualUserParser(IEnumerable<ICommandContext> commandContext, IDiscordRestUserAPI userApi)
        {
            _userApi = userApi;
            _commandContext = commandContext.FirstOrDefault();
        }

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