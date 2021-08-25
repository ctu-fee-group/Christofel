using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Results;

namespace Christofel.BaseLib.Implementations.Responders
{
    public abstract class EveryResponder : IEveryResponder
    {
        public abstract Task<Result> RespondAnyAsync<TEvent>(TEvent gatewayEvent, CancellationToken ct = default)
            where TEvent : IGatewayEvent;

        public Task<Result> RespondAsync(IChannelDelete gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IChannelPinsUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IChannelUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IStageInstanceCreate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IStageInstanceDelete gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IStageInstanceUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IThreadCreate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IThreadDelete gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IThreadListSync gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IThreadMemberUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IThreadMembersUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IThreadUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IHello gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IInvalidSession gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IReconnect gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IResumed gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildBanAdd gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildBanRemove gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildDelete gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildEmojisUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildIntegrationsUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildMemberRemove gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildMemberUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildMembersChunk gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildRoleCreate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildRoleDelete gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildRoleUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildStickersUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IGuildUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IIntegrationCreate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IIntegrationUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IIntegrationDelete gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IInviteCreate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IInviteDelete gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IMessageDelete gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IMessageDeleteBulk gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IMessageReactionAdd gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IMessageReactionRemove gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IMessageReactionRemoveAll gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IMessageReactionRemoveEmoji gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IMessageUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IPresenceUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(ITypingStart gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IUserUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IVoiceServerUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IVoiceStateUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IWebhooksUpdate gatewayEvent, CancellationToken ct = default)
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IChannelCreate gatewayEvent, CancellationToken ct = new CancellationToken())
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IApplicationCommandCreate gatewayEvent, CancellationToken ct = new CancellationToken())
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IApplicationCommandDelete gatewayEvent, CancellationToken ct = new CancellationToken())
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IApplicationCommandUpdate gatewayEvent, CancellationToken ct = new CancellationToken())
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }

        public Task<Result> RespondAsync(IInteractionCreate gatewayEvent, CancellationToken ct = new CancellationToken())
        {
            return RespondAnyAsync(gatewayEvent, ct);
        }
    }
}