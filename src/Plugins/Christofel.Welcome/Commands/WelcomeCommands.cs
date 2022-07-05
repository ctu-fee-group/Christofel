//
//  WelcomeCommands.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Welcome.Commands;

/// <summary>
/// Handles welcome message creation.
/// </summary>
[Group("welcome")]
[Description("Sends welcome message with interactive buttons.")]
[Ephemeral]
public class WelcomeCommands : CommandGroup
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly FeedbackService _feedbackService;
    private readonly ICommandContext _context;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly WelcomeOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WelcomeCommands"/> class.
    /// </summary>
    /// <param name="channelApi">The channel api.</param>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="context">The context.</param>
    /// <param name="options">The options.</param>
    /// <param name="jsonOptions">The json options.</param>
    public WelcomeCommands
    (
        IDiscordRestChannelAPI channelApi,
        FeedbackService feedbackService,
        ICommandContext context,
        IOptionsSnapshot<WelcomeOptions> options,
        IOptionsSnapshot<JsonSerializerOptions> jsonOptions
    )
    {
        _channelApi = channelApi;
        _feedbackService = feedbackService;
        _context = context;
        _jsonOptions = jsonOptions.Get("Discord");
        _options = options.Value;
    }

    /// <summary>
    /// Sends the welcome message.
    /// </summary>
    /// <param name="language">The language of the welcome message to send.</param>
    /// <param name="channel">The channel to send welcome message to.</param>
    /// <returns>A result that may have failed.</returns>
    [Command("send")]
    [Ephemeral]
    public async Task<Result> HandleSendWelcomeAsync(string? language = default, Snowflake? channel = default)
    {
        language ??= _options.DefaultLanguage;

        if (!_options.Translations.ContainsKey(language))
        {
            await _feedbackService.SendContextualErrorAsync
                ("The given translation does not exist.", ct: CancellationToken);
            return new NotFoundError($"Could not find translation of welcome to {language}");
        }

        var translation = _options.Translations[language];
        if (!File.Exists(translation.EmbedFilePath))
        {
            return new NotFoundError("Could not find welcome embed file.");
        }

        var embed = JsonSerializer.Deserialize<IEmbed>
            (await File.ReadAllTextAsync(translation.EmbedFilePath, CancellationToken), _jsonOptions);
        if (embed is null)
        {
            // error
            return new GenericError("Welcome embed string could not be deserialized into an embed.");
        }

        var messageResult = await _channelApi.CreateMessageAsync
        (
            channel ?? _context.ChannelID,
            embeds: new[] { embed },
            components: WelcomeMessageHelper.CreateWelcomeComponents(_options, language),
            ct: CancellationToken
        );

        if (messageResult.IsSuccess)
        {
            var feedbackResult = await _feedbackService.SendContextualSuccessAsync
                ("The welcome message was sent.", ct: CancellationToken);
            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        await _feedbackService.SendContextualErrorAsync
            ("The welcome message could not be sent.", ct: CancellationToken);

        return messageResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(messageResult);
    }

    /// <summary>
    /// Updates the welcome message.
    /// </summary>
    /// <param name="messageId">The id of the message to update.</param>
    /// <param name="language">The language of the welcome message to send.</param>
    /// <param name="channel">The channel to send welcome message to.</param>
    /// <returns>A result that may have failed.</returns>
    [Command("update")]
    [Ephemeral]
    public async Task<Result> HandleUpdateWelcomeAsync
    (
        [DiscordTypeHint(TypeHint.String)]
        Snowflake messageId,
        string? language = default,
        Snowflake? channel = default
    )
    {
        language ??= _options.DefaultLanguage;

        if (!_options.Translations.ContainsKey(language))
        {
            await _feedbackService.SendContextualErrorAsync
                ("The given translation does not exist.", ct: CancellationToken);
            return new NotFoundError($"Could not find translation of welcome to {language}");
        }

        var translation = _options.Translations[language];
        if (!File.Exists(translation.EmbedFilePath))
        {
            return new NotFoundError("Could not find welcome embed file.");
        }

        var embed = JsonSerializer.Deserialize<IEmbed>
            (await File.ReadAllTextAsync(translation.EmbedFilePath, CancellationToken), _jsonOptions);
        if (embed is null)
        {
            return new GenericError("Welcome embed string could not be deserialized into an embed.");
        }

        var messageResult = await _channelApi.EditMessageAsync
        (
            channel ?? _context.ChannelID,
            messageId,
            embeds: new[] { embed },
            components: WelcomeMessageHelper.CreateWelcomeComponents(_options, language),
            ct: CancellationToken
        );

        if (messageResult.IsSuccess)
        {
            var feedbackResult = await _feedbackService.SendContextualSuccessAsync
                ("The welcome message was updated.", ct: CancellationToken);
            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        await _feedbackService.SendContextualErrorAsync
            ("The welcome message could not be updated.", ct: CancellationToken);

        return messageResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(messageResult);
    }
}