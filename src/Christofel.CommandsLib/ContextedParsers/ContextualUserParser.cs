using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Parsers;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Parsers;
using Remora.Discord.Core;
using Remora.Results;
using IUser = Remora.Discord.API.Abstractions.Objects.IUser;

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
        
        public override ValueTask<Result<IUser>> TryParse(string value, CancellationToken ct)
        {
            if (_commandContext is InteractionContext interactionContext &&
                Snowflake.TryParse(value, out var userID) &&
                interactionContext.Data.Resolved.IsDefined(out var resolved) &&
                resolved.Users.IsDefined(out var users) &&
                users.TryGetValue(userID.Value, out var user))
            {
                return ValueTask.FromResult(Result<IUser>.FromSuccess(user));
            }

            return new UserParser(_userApi).TryParse(value, ct);
        }
    }
}