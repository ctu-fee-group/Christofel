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
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Welcome.Commands;

/// <summary>
/// Handles welcome message creation.
/// </summary>
public class WelcomeCommands : CommandGroup
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly WelcomeMessageHelper _welcomeMessageHelper;
    private readonly FeedbackService _feedbackService;
    private readonly ICommandContext _context;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly WelcomeOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WelcomeCommands"/> class.
    /// </summary>
    /// <param name="channelApi">The channel api.</param>
    /// <param name="welcomeMessageHelper">The message helper.</param>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="context">The context.</param>
    /// <param name="options">The options.</param>
    /// <param name="jsonOptions">The json options.</param>
    public WelcomeCommands
    (
        IDiscordRestChannelAPI channelApi,
        WelcomeMessageHelper welcomeMessageHelper,
        FeedbackService feedbackService,
        ICommandContext context,
        IOptionsSnapshot<WelcomeOptions> options,
        IOptionsSnapshot<JsonSerializerOptions> jsonOptions
    )
    {
        _channelApi = channelApi;
        _welcomeMessageHelper = welcomeMessageHelper;
        _feedbackService = feedbackService;
        _context = context;
        _jsonOptions = jsonOptions.Get("Discord");
        _options = options.Value;
    }

    /// <summary>
    /// Sends the welcome message.
    /// </summary>
    /// <param name="channel">The channel to send welcome message to.</param>
    /// <returns>A result that may have failed.</returns>
    [Command("sendwelcome")]
    [Description("Sends welcome message with interactive buttons.")]
    public async Task<Result> HandleSendWelcomeAsync(Snowflake? channel = null)
    {
        if (!File.Exists(_options.WelcomeEmbedFile))
        {
            return new NotFoundError("Could not find welcome embed file.");
        }

        var embed = JsonSerializer.Deserialize<IEmbed>
            (await File.ReadAllTextAsync(_options.WelcomeEmbedFile, CancellationToken), _jsonOptions);
        if (embed is null)
        {
            // error
            return new GenericError("Welcome embed string could not be deserialized into an embed.");
        }

        var messageResult = await _channelApi.CreateMessageAsync
        (
            channel ?? _context.ChannelID,
            embeds: new[] { embed },
            components: WelcomeMessageHelper.CreateWelcomeComponents(_options),
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
}