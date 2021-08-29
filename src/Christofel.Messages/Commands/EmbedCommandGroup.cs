using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib;
using Christofel.Messages.Services;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Messages.Commands
{
    [Group("embed")]
    [Description("Manage embeds")]
    [RequirePermission("messages.embed")]
    [Ephemeral]
    [DiscordDefaultPermission(false)]
    public class EmbedCommandGroup : CommandGroup
    {
        private static async Task<Result> HandleEditEmbed(FeedbackService feedbackService,
            IDiscordRestChannelAPI channelApi,
            Snowflake channelId, Snowflake messageId, Embed embed, CancellationToken ct)
        {
            var messageResult =
                await channelApi.EditMessageAsync(channelId, messageId, embeds: new[] { embed }, ct: ct);
            if (!messageResult.IsSuccess)
            {
                await feedbackService.SendContextualErrorAsync("Could not edit the embed, check permissions",
                    ct: ct);
                return Result.FromError(messageResult);
            }

            var feedbackResult = await feedbackService.SendContextualSuccessAsync("Embed edited", ct: ct);

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        private static async Task<Result> HandleCreateEmbed(FeedbackService feedbackService,
            Snowflake channelId, Embed embed, CancellationToken ct)
        {
            var messageResult = await feedbackService.SendEmbedAsync(channelId, embed, ct);
            if (!messageResult.IsSuccess)
            {
                await feedbackService.SendContextualErrorAsync("Could not send the embed, check permissions", ct: ct);
                return Result.FromError(messageResult);
            }

            var feedbackResult = await feedbackService.SendContextualSuccessAsync("Embed sent", ct: ct);

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        [Group("file")]
        [Description("Manage embeds using json files in the filesystem")]
        [RequirePermission("messages.embed.file")]
        public class FileInner : CommandGroup
        {
            private readonly ILogger<ReactCommandGroup> _logger;
            private readonly EmbedsProvider _embeds;
            private readonly FeedbackService _feedbackService;
            private readonly ICommandContext _context;
            private readonly IDiscordRestChannelAPI _channelApi;


            public FileInner(ILogger<ReactCommandGroup> logger, EmbedsProvider embeds,
                FeedbackService feedbackService, ICommandContext context, IDiscordRestChannelAPI channelApi)
            {
                _channelApi = channelApi;
                _feedbackService = feedbackService;
                _context = context;
                _embeds = embeds;
                _logger = logger;
            }
            
            private async Task<Result<Embed>> ParseEmbed(string file)
            {
                try
                {
                    var embed = await _embeds.GetEmbedFromFile(file);
                    if (embed is null)
                    {
                        return new InvalidOperationError("Could not parse the embed");
                    }

                    return Result<Embed>.FromSuccess(embed);
                }
                catch (Exception e)
                {
                    await _feedbackService.SendContextualErrorAsync($"Could not parse the embed: {e.Message}");
                    return e;
                }
            }
            
            [Command("edit")]
            [Description("Edit an embed from json file")]
            [RequirePermission("messages.embed.file.edit")]
            public async Task<Result> HandleEditEmbedFromFile(
                [Description("What message to edit"), DiscordTypeHint(TypeHint.String)]
                Snowflake messageId,
                [Description("File with json to send")]
                string embed,
                [Description("Where to send the message. Default current channel"), DiscordTypeHint(TypeHint.Channel)]
                Snowflake? channel = default)
            {
                var parseResult = await ParseEmbed(embed);
                if (!parseResult.IsSuccess)
                {
                    return Result.FromError(parseResult);
                }

                return await HandleEditEmbed(_feedbackService, _channelApi,
                    channel ?? _context.ChannelID,
                    messageId, parseResult.Entity, CancellationToken);
            }


            [Command("send")]
            [Description("Create an embed from json file")]
            [RequirePermission("messages.embed.file.send")]
            public async Task<Result> HandleCreateEmbedFromFile(
                [Description("File with json to send")] string embed,
                [Description("Where to send the message. Default current channel"), DiscordTypeHint(TypeHint.Channel)]
                Snowflake? channel = null)
            {
                var parseResult = await ParseEmbed(embed);
                if (!parseResult.IsSuccess)
                {
                    return Result.FromError(parseResult);
                }

                return await HandleCreateEmbed(_feedbackService, channel ?? _context.ChannelID,
                    parseResult.Entity, CancellationToken);
            }
        }

        [Group("msg")]
        [Description("Manage embeds using json messages")]
        [RequirePermission("messages.embed.msg")]
        public class MessageInner : CommandGroup
        {
            private readonly ILogger<ReactCommandGroup> _logger;
            private readonly EmbedsProvider _embeds;
            private readonly FeedbackService _feedbackService;
            private readonly ICommandContext _context;
            private readonly IDiscordRestChannelAPI _channelApi;


            public MessageInner(ILogger<ReactCommandGroup> logger, EmbedsProvider embeds,
                FeedbackService feedbackService, ICommandContext context, IDiscordRestChannelAPI channelApi)
            {
                _channelApi = channelApi;
                _feedbackService = feedbackService;
                _context = context;
                _embeds = embeds;
                _logger = logger;
            }

            private async Task<Result<Embed>> ParseEmbed(string embedString)
            {
                try
                {
                    var embed = _embeds.GetEmbedFromString(embedString);
                    if (embed is null)
                    {
                        return new InvalidOperationError("Could not parse the embed");
                    }

                    return Result<Embed>.FromSuccess(embed);
                }
                catch (Exception e)
                {
                    await _feedbackService.SendContextualErrorAsync($"Could not parse the embed: {e.Message}");
                    return e;
                }
            }

            [Command("edit")]
            [Description("Edit message with embed from json string")]
            [RequirePermission("messages.embed.msg.edit")]
            public async Task<Result> HandleEditEmbedFromMessage(
                [Description("What message to edit"), DiscordTypeHint(TypeHint.String)]
                Snowflake messageId,
                [Description("Embed json"), Greedy]
                string embed,
                [Description("Where to send the message. Default current channel"), DiscordTypeHint(TypeHint.Channel)]
                Snowflake? channel = null)
            {
                var parseResult = await ParseEmbed(embed);
                if (!parseResult.IsSuccess)
                {
                    return Result.FromError(parseResult);
                }

                return await HandleEditEmbed(_feedbackService, _channelApi,
                    channel ?? _context.ChannelID,
                    messageId, parseResult.Entity, CancellationToken);
            }

            [RequirePermission("messages.embed.msg.send")]
            [Command("send")]
            [Description("Create an embed from json string")]
            public async Task<Result> HandleCreateEmbedFromMessage(
                [Description("Embed json to send"), Greedy]
                string embed,
                [Description("Where to send the message. Default current channel"), DiscordTypeHint(TypeHint.Channel)]
                Snowflake? channel = default)
            {
                var parseResult = await ParseEmbed(embed);
                if (!parseResult.IsSuccess)
                {
                    return Result.FromError(parseResult);
                }

                return await HandleCreateEmbed(_feedbackService, channel ?? _context.ChannelID,
                    parseResult.Entity, CancellationToken);
            }
        }
    }
}