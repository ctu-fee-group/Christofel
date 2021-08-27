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
    public interface IGuildMemberOrRole
    {
        public IUser? User { get; }

        public IPartialGuildMember? Member { get; }

        public IRole? Role { get; }
    }

    public record GuildMemberOrRole(IUser? User, IPartialGuildMember? Member, IRole? Role) : IGuildMemberOrRole;

    public class ContextualMemberOrRoleParser : AbstractTypeParser<IGuildMemberOrRole>
    {
        private readonly ICommandContext _commandContext;

        public ContextualMemberOrRoleParser(ICommandContext commandContext)
        {
            _commandContext = commandContext;
        }

        public override ValueTask<Result<IGuildMemberOrRole>> TryParse(string value, CancellationToken ct)
        {
            if (!Snowflake.TryParse(value, out var unknownID))
            {
                return ValueTask.FromResult<Result<IGuildMemberOrRole>>(new ParsingError<IGuildMemberOrRole>(value));
            }

            IRole? role = null;
            IUser? user = null;
            IPartialGuildMember? member = null;

            bool success = false;
            if (_commandContext is InteractionContext interactionContext &&
                interactionContext.Data.Resolved.IsDefined(out var resolved))
            {
                if (resolved.Roles.IsDefined(out var roles) &&
                    roles.TryGetValue(unknownID.Value, out role))
                { 
                    success = true;
                }

                if (resolved.Members.IsDefined(out var members) &&
                    members.TryGetValue(unknownID.Value, out member))
                {
                    success = true;
                }

                if (resolved.Users.IsDefined(out var users) &&
                    users.TryGetValue(unknownID.Value, out user)) ;
            }

            // TODO: maybe try to get user or role from the API if it isn't found in the resolved data?
            
            return ValueTask.FromResult<Result<IGuildMemberOrRole>>(
                success 
                    ? Result<IGuildMemberOrRole>.FromSuccess(new GuildMemberOrRole(user, member, role))
                    : new ParsingError<IGuildMemberOrRole>("Could not find matching user or role"));
        }
    }
}