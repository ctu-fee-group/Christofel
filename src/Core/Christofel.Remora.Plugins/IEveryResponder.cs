//
//   IEveryResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;

namespace Christofel.Remora
{
#pragma warning disable SA1402 // FileMayOnlyContainASingleType
    /// <summary>
    /// Implements every possible <see cref="IGatewayEvent"/> responder.
    /// </summary>
    public interface IEveryResponder :
        IChannelsResponder,
        IConnectingResumingResponder,
        IGuildResponder,
        IIntegrationResponder,
        IInteractionsResponder,
        IInvitesResponder,
        IMessagesResponder,
        IPresenceResponder,
        IUsersResponder,
        IVoiceResponder,
        IWebhooksResponder
    {
    }

    /// <summary>
    /// Implements <see cref="IGatewayEvent"/> responders for every webhook event.
    /// </summary>
    public interface IWebhooksResponder :
        IResponder<IWebhooksUpdate>
    {
    }

    /// <summary>
    /// Implements <see cref="IGatewayEvent"/> responders for every voice event.
    /// </summary>
    public interface IVoiceResponder :
        IResponder<IVoiceServerUpdate>,
        IResponder<IVoiceStateUpdate>
    {
    }

    /// <summary>
    /// Implements <see cref="IGatewayEvent"/> responders for every users event.
    /// </summary>
    public interface IUsersResponder :
        IResponder<ITypingStart>,
        IResponder<IUserUpdate>
    {
    }

    /// <summary>
    /// Implements <see cref="IGatewayEvent"/> responders for every presence event.
    /// </summary>
    public interface IPresenceResponder :
        IResponder<IPresenceUpdate>
    {
    }

    /// <summary>
    /// Implements <see cref="IGatewayEvent"/> responders for every message event.
    /// </summary>
    public interface IMessagesResponder :
        IResponder<IMessageCreate>,
        IResponder<IMessageDelete>,
        IResponder<IMessageDeleteBulk>,
        IResponder<IMessageReactionAdd>,
        IResponder<IMessageReactionRemove>,
        IResponder<IMessageReactionRemoveAll>,
        IResponder<IMessageReactionRemoveEmoji>,
        IResponder<IMessageUpdate>
    {
    }

    /// <summary>
    /// Implements <see cref="IGatewayEvent"/> responders for every invite event.
    /// </summary>
    public interface IInvitesResponder :
        IResponder<IInviteCreate>,
        IResponder<IInviteDelete>
    {
    }

    /// <summary>
    /// Implements <see cref="IGatewayEvent"/> responders for every interaction event.
    /// </summary>
    public interface IInteractionsResponder :
        IResponder<IInteractionCreate>
    {
    }

    /// <summary>
    /// Implements <see cref="IGatewayEvent"/> responders for every integration event.
    /// </summary>
    public interface IIntegrationResponder :
        IResponder<IIntegrationCreate>,
        IResponder<IIntegrationUpdate>,
        IResponder<IIntegrationDelete>
    {
    }

    /// <summary>
    /// Implements <see cref="IGatewayEvent"/> responders for every guild event.
    /// </summary>
    public interface IGuildResponder :
        IResponder<IGuildBanAdd>,
        IResponder<IGuildBanRemove>,
        IResponder<IGuildCreate>,
        IResponder<IGuildDelete>,
        IResponder<IGuildEmojisUpdate>,
        IResponder<IGuildIntegrationsUpdate>,
        IResponder<IGuildMemberAdd>,
        IResponder<IGuildMemberRemove>,
        IResponder<IGuildMemberUpdate>,
        IResponder<IGuildMembersChunk>,
        IResponder<IGuildRoleCreate>,
        IResponder<IGuildRoleDelete>,
        IResponder<IGuildRoleUpdate>,
        IResponder<IGuildStickersUpdate>,
        IResponder<IGuildUpdate>
    {
    }

    /// <summary>
    /// Implements <see cref="IGatewayEvent"/> responders for every connecting or resuming event.
    /// </summary>
    public interface IConnectingResumingResponder :
        IResponder<IHello>,
        IResponder<IInvalidSession>,
        IResponder<IReady>,
        IResponder<IReconnect>,
        IResponder<IResumed>
    {
    }

    /// <summary>
    /// Implements <see cref="IGatewayEvent"/> responders for every channel event.
    /// </summary>
    public interface IChannelsResponder :
        IResponder<IChannelCreate>,
        IResponder<IChannelDelete>,
        IResponder<IChannelPinsUpdate>,
        IResponder<IChannelUpdate>,
        IResponder<IStageInstanceCreate>,
        IResponder<IStageInstanceDelete>,
        IResponder<IStageInstanceUpdate>,
        IResponder<IThreadCreate>,
        IResponder<IThreadDelete>,
        IResponder<IThreadListSync>,
        IResponder<IThreadMemberUpdate>,
        IResponder<IThreadMembersUpdate>,
        IResponder<IThreadUpdate>
    {
    }
}