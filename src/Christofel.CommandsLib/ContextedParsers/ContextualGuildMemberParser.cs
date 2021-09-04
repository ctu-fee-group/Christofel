using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Parsers;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.CommandsLib.ContextedParsers
{
    public class ContextualGuildMemberParser : AbstractTypeParser<IPartialGuildMember>
    {
        private readonly ICommandContext _commandContext;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly IDiscordRestChannelAPI _channelApi;
        
        public ContextualGuildMemberParser(ICommandContext commandContext, IDiscordRestChannelAPI channelApi, IDiscordRestGuildAPI guildApi)
        {
            _guildApi = guildApi;
            _channelApi = channelApi;
            _commandContext = commandContext;
        }
        
        public override async ValueTask<Result<IPartialGuildMember>> TryParseAsync(string value, CancellationToken ct)
        {
            PartialGuildMember? retrievedMember = null;
            
            if (_commandContext is InteractionContext interactionContext &&
                Snowflake.TryParse(value, out var guildMemberID) &&
                interactionContext.Data.Resolved.IsDefined(out var resolved))
            {
                if (resolved.Members.IsDefined(out var members) &&
                    members.TryGetValue(guildMemberID.Value, out var member))
                {
                    retrievedMember = new PartialGuildMember(member.User, member.Nickname, member.Roles,
                        member.JoinedAt,
                        member.PremiumSince,
                        member.IsDeafened,
                        member.IsMuted,
                        member.IsPending,
                        member.Permissions);
                }
                
                if (retrievedMember is not null &&
                    resolved.Users.IsDefined(out var users) &&
                    users.TryGetValue(guildMemberID.Value, out var user))
                {
                    retrievedMember = retrievedMember with
                    {
                        User = new Optional<IUser>(user)
                    };
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
                    await new GuildMemberParser(_commandContext, _channelApi, _guildApi).TryParseAsync(value, ct);

                if (!parsed.IsSuccess)
                {
                    return Result<IPartialGuildMember>.FromError(parsed);
                }
                
                retrievedMember = new PartialGuildMember(parsed.Entity.User,
                    parsed.Entity.Nickname, 
                    new Optional<IReadOnlyList<Snowflake>>(parsed.Entity.Roles),
                    parsed.Entity.JoinedAt, 
                    parsed.Entity.PremiumSince,
                    parsed.Entity.IsDeafened,
                    parsed.Entity.IsMuted,
                    parsed.Entity.IsPending,
                    parsed.Entity.Permissions);
            }

            return Result<IPartialGuildMember>.FromSuccess(retrievedMember);
        }
    }
}