//
//   IEveryResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;

namespace Christofel.Remora
{
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

    public interface IWebhooksResponder :
        IResponder<IWebhooksUpdate>
    {
    }

    public interface IVoiceResponder :
        IResponder<IVoiceServerUpdate>,
        IResponder<IVoiceStateUpdate>
    {
    }

    public interface IUsersResponder :
        IResponder<ITypingStart>,
        IResponder<IUserUpdate>
    {
    }

    public interface IPresenceResponder :
        IResponder<IPresenceUpdate>
    {
    }

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

    public interface IInvitesResponder :
        IResponder<IInviteCreate>,
        IResponder<IInviteDelete>
    {
    }

    public interface IInteractionsResponder :
        IResponder<IInteractionCreate>
    {
    }

    public interface IIntegrationResponder :
        IResponder<IIntegrationCreate>,
        IResponder<IIntegrationUpdate>,
        IResponder<IIntegrationDelete>
    {
    }

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

    public interface IConnectingResumingResponder :
        IResponder<IHello>,
        IResponder<IInvalidSession>,
        IResponder<IReady>,
        IResponder<IReconnect>,
        IResponder<IResumed>
    {
    }

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