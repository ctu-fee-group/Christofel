//
//  WelcomeMessage.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Welcome;

/// <summary>
/// A service for sending welcome message.
/// </summary>
public class WelcomeMessage
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly WelcomeOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WelcomeMessage"/> class.
    /// </summary>
    /// <param name="channelApi">The channel api.</param>
    /// <param name="options">The welcome message options.</param>
    /// <param name="jsonOptions">The json options.</param>
    public WelcomeMessage
    (
        IDiscordRestChannelAPI channelApi,
        IOptionsSnapshot<WelcomeOptions> options,
        IOptionsSnapshot<JsonSerializerOptions> jsonOptions
    )
    {
        _channelApi = channelApi;
        _jsonOptions = jsonOptions.Get("Discord");
        _options = options.Value;
    }

    /// <summary>
    /// Send welcome message to the given channel.
    /// </summary>
    /// <param name="channelId">The channel to send the welcome message to.</param>
    /// <param name="language">The language of the welcome message. Null for default language.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>The sent message, or an error.</returns>
    public async Task<Result<IMessage>> SendWelcomeMessage(Snowflake channelId, string? language = default, CancellationToken ct = default)
    {
        language ??= _options.DefaultLanguage;

        if (!_options.Translations.ContainsKey(language))
        {
            return new NotFoundError($"Could not find translation of welcome to {language}");
        }

        var translation = _options.Translations[language];
        if (!File.Exists(translation.EmbedFilePath))
        {
            return new NotFoundError("Could not find welcome embed file.");
        }

        var embed = JsonSerializer.Deserialize<IEmbed>
            (await File.ReadAllTextAsync(translation.EmbedFilePath, ct), _jsonOptions);
        if (embed is null)
        {
            // error
            return new GenericError("Welcome embed string could not be deserialized into an embed.");
        }

        return await _channelApi.CreateMessageAsync
        (
            channelId,
            embeds: new[] { embed },
            components: WelcomeMessageHelper.CreateWelcomeComponents(_options, language),
            ct: ct
        );
    }
}