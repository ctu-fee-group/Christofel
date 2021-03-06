//
//   ContextualChannelParser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Parsers;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.CommandsLib.ContextedParsers
{
    /// <summary>
    /// Parses <see cref="IPartialChannel"/> by using <see cref="ICommandContext"/>.
    /// </summary>
    /// <remarks>
    /// For interaction context, the data will be obtained from <see cref="IInteractionData.Resolved"/>,
    /// if it cannot be found there, error will be returned.
    ///
    /// For message context, the channel will be loaded using channel api.
    ///
    /// Slash commands have to be executed
    /// with parameters using mentions, not by ids of the objects
    /// as that won't put them into Resolved.
    /// </remarks>
    public class ContextualChannelParser : AbstractTypeParser<IPartialChannel>
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommandContext _commandContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextualChannelParser"/> class.
        /// </summary>
        /// <param name="commandContext">The context of the current command.</param>
        /// <param name="channelApi">The api for getting information about channels.</param>
        public ContextualChannelParser(ICommandContext commandContext, IDiscordRestChannelAPI channelApi)
        {
            _channelApi = channelApi;
            _commandContext = commandContext;
        }

        /// <inheritdoc />
        public override async ValueTask<Result<IPartialChannel>> TryParseAsync(string value, CancellationToken ct = default)
        {
            if (_commandContext is InteractionContext interactionContext &&
                Snowflake.TryParse(value.Unmention(), out var channelID) &&
                interactionContext.Data.TryPickT0(out var data, out _) &&
                data.Resolved.IsDefined(out var resolved) &&
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

            return new PartialChannel
            (
                result.Entity.ID,
                result.Entity.Type,
                result.Entity.GuildID,
                result.Entity.Position,
                result.Entity.PermissionOverwrites,
                result.Entity.Name,
                result.Entity.Topic,
                result.Entity.IsNsfw,
                result.Entity.LastMessageID,
                result.Entity.Bitrate,
                result.Entity.UserLimit,
                result.Entity.RateLimitPerUser,
                result.Entity.Recipients,
                result.Entity.Icon,
                result.Entity.OwnerID,
                result.Entity.ApplicationID,
                result.Entity.ParentID,
                result.Entity.LastPinTimestamp,
                result.Entity.RTCRegion,
                result.Entity.VideoQualityMode,
                result.Entity.MessageCount,
                result.Entity.MemberCount,
                result.Entity.ThreadMetadata,
                result.Entity.Member,
                result.Entity.DefaultAutoArchiveDuration,
                result.Entity.Permissions
            );
        }
    }
}