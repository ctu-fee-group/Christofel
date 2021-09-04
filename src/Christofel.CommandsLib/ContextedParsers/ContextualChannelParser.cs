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
    public class ContextualChannelParser : AbstractTypeParser<IPartialChannel>
    {
        private readonly ICommandContext _commandContext;
        private readonly IDiscordRestChannelAPI _channelApi;

        public ContextualChannelParser(ICommandContext commandContext, IDiscordRestChannelAPI channelApi)
        {
            _channelApi = channelApi;
            _commandContext = commandContext;
        }

        public override async ValueTask<Result<IPartialChannel>> TryParseAsync(string value, CancellationToken ct)
        {
            if (_commandContext is InteractionContext interactionContext &&
                Snowflake.TryParse(value, out var channelID) &&
                interactionContext.Data.Resolved.IsDefined(out var resolved) &&
                resolved.Channels.IsDefined(out var channels) &&
                channels.TryGetValue(channelID.Value, out var channel))
            {
                return Result<IPartialChannel>.FromSuccess(channel);
            }

            if (_commandContext is InteractionContext)
            {
                return new ParsingError<IPartialChannel>("Could not find specified channel in resolved data");
            }

            var result = await new ChannelParser(_channelApi).TryParseAsync(value, ct);

            if (!result.IsSuccess)
            {
                return Result<IPartialChannel>.FromError(result);
            }

            return new PartialChannel(result.Entity.ID, result.Entity.Type, result.Entity.GuildID,
                result.Entity.Position, result.Entity.PermissionOverwrites, result.Entity.Name, result.Entity.Topic,
                result.Entity.IsNsfw, result.Entity.LastMessageID, result.Entity.Bitrate, result.Entity.UserLimit,
                result.Entity.RateLimitPerUser, result.Entity.Recipients, result.Entity.Icon, result.Entity.OwnerID,
                result.Entity.ApplicationID, result.Entity.ParentID, result.Entity.LastPinTimestamp,
                result.Entity.RTCRegion, result.Entity.VideoQualityMode, result.Entity.MessageCount,
                result.Entity.MemberCount, result.Entity.ThreadMetadata, result.Entity.Member,
                result.Entity.DefaultAutoArchiveDuration, result.Entity.Permissions);
        }
    }
}