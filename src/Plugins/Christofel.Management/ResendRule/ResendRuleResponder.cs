//
//   ResendRuleResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Common.Database;
using Christofel.Helpers.Helpers;
using Christofel.Management.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OneOf;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Discord.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Constants = Remora.Discord.API.Constants;

namespace Christofel.Management.ResendRule
{
    /// <summary>
    /// Resends messages from channels according to <see cref="Database.Models.ResendRule"/>.
    /// </summary>
    public class ResendRuleResponder : IResponder<IMessageCreate>, IResponder<IMessageDelete>
    {
        private readonly IReadableDbContext<ManagementContext> _dbCcontext;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _client;
        private readonly ResendRuleOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResendRuleResponder"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="options">The options for the resend messages.</param>
        /// <param name="channelApi">The channel api.</param>
        /// <param name="memoryCache">The memory cache.</param>
        /// <param name="client">The http client.</param>
        public ResendRuleResponder
        (
            IReadableDbContext<ManagementContext> context,
            IOptionsSnapshot<ResendRuleOptions> options,
            IDiscordRestChannelAPI channelApi,
            IMemoryCache memoryCache,
            HttpClient client
        )
        {
            _dbCcontext = context;
            _channelApi = channelApi;
            _memoryCache = memoryCache;
            _client = client;
            _options = options.Value;
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
        {
            if (gatewayEvent.Author.IsBot.IsDefined(out var isBot) && isBot)
            {
                return Result.FromSuccess();
            }

            var resendRules = await _dbCcontext.Set<Database.Models.ResendRule>()
                .Where(x => x.FromChannel == gatewayEvent.ChannelID)
                .ToListAsync(ct);

            if (resendRules.Count == 0)
            {
                return Result.FromSuccess();
            }

            var channelResult = await _channelApi.GetChannelAsync(gatewayEvent.ChannelID, ct);
            if (!channelResult.IsSuccess)
            {
                return Result.FromError(channelResult);
            }

            if (channelResult.Entity.Type == ChannelType.GuildNews)
            {
                var crosspostResponse = await _channelApi.CrosspostMessageAsync
                    (gatewayEvent.ChannelID, gatewayEvent.ID, ct);

                if (!crosspostResponse.IsSuccess)
                {
                    return Result.FromError(crosspostResponse);
                }
            }

            string sendMessageContent = _options.Format
                .Replace("{header}", _options.Header)
                .Replace("{message}", gatewayEvent.Content)
                .Replace("{channel}", $"<#{channelResult.Entity.ID}>")
                .Replace("{mention}", $"<@{gatewayEvent.Author.ID}>");

            List<(Snowflake Channel, Snowflake Message)> resentMessages =
                new List<(Snowflake Channel, Snowflake Message)>();

            var attachments = new List<OneOf<FileData, IPartialAttachment>>();
            foreach (var attachment in gatewayEvent.Attachments)
            {
                var response = await _client.GetAsync(attachment.ProxyUrl, cancellationToken: ct);
                if (!response.IsSuccessStatusCode)
                {
                    return new GenericError(response.ReasonPhrase ?? "No reason given.");
                }

                var responseStream = await response.Content.ReadAsStreamAsync(ct);
                attachments.Add
                (
                    OneOf<FileData, IPartialAttachment>.FromT0
                    (
                        new FileData
                        (
                            attachment.Filename,
                            responseStream,
                            attachment.Description.HasValue ? attachment.Description.Value : "No description"
                        )
                    )
                );
            }

            foreach (var resendRule in resendRules)
            {
                var createdResult = await _channelApi.CreateMessageAsync
                (
                    resendRule.ToChannel,
                    content: sendMessageContent,
                    allowedMentions: AllowedMentionsHelper.None,
                    attachments: new Optional<IReadOnlyList<OneOf<FileData, IPartialAttachment>>>(attachments),
                    embeds: new Optional<IReadOnlyList<IEmbed>>(gatewayEvent.Embeds),
                    ct: ct
                );

                if (!createdResult.IsSuccess)
                {
                    return Result.FromError(createdResult);
                }

                resentMessages.Add((resendRule.ToChannel, createdResult.Entity.ID));
            }

            foreach (var attachment in attachments)
            {
                await attachment.AsT0.Content.DisposeAsync();
            }

            var messageKey = GetMessageKey(gatewayEvent.ChannelID, gatewayEvent.ID);
            var metadata = new ResendRuleMetadata(gatewayEvent.ChannelID, gatewayEvent.ID, resentMessages.ToArray());
            _memoryCache.Set
            (
                messageKey,
                metadata,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.CacheDuration),
                }
            );

            return Result.FromSuccess();
        }

        /// <inheritdoc />
        public async Task<Result> RespondAsync(IMessageDelete gatewayEvent, CancellationToken ct = default)
        {
            var messageKey = GetMessageKey(gatewayEvent.ChannelID, gatewayEvent.ID);
            var metadata = _memoryCache.Get<ResendRuleMetadata?>(messageKey);

            if (metadata is not null)
            {
                foreach (var resentMessage in metadata.ResentMessages)
                {
                    var deleteResult = await _channelApi.DeleteMessageAsync
                        (resentMessage.Channel, resentMessage.Message, "Bound resend source message was deleted.", ct);
                    if (!deleteResult.IsSuccess)
                    {
                        return deleteResult;
                    }
                }

                _memoryCache.Remove(messageKey);
            }

            return Result.FromSuccess();
        }

        private string GetMessageKey(Snowflake channel, Snowflake message) => $"Resend_{channel}:{message}";
    }
}