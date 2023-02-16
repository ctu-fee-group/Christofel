//
//   AutoThreadsResponder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Christofel.Enhancements.AutoThreads;

/// <summary>
/// Creates threads when message sent in given channel.
/// </summary>
public class AutoThreadsResponder : IResponder<IMessageCreate>
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly ILogger<AutoThreadsResponder> _logger;
    private readonly AutoThreadsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoThreadsResponder"/> class.
    /// </summary>
    /// <param name="channelApi">The channel api.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    public AutoThreadsResponder
    (
        IDiscordRestChannelAPI channelApi,
        ILogger<AutoThreadsResponder> logger,
        IOptionsSnapshot<AutoThreadsOptions> options
    )
    {
        _channelApi = channelApi;
        _logger = logger;
        _options = options.Value;

    }

    /// <inheritdoc />
    public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.Author.IsBot.IsDefined(out var bot) && bot)
        {
            return Result.FromSuccess();
        }

        if (!_options.Channels.Contains(gatewayEvent.ChannelID.Value))
        {
            return Result.FromSuccess();
        }

        if (gatewayEvent.ReferencedMessage.IsDefined(out var message))
        {
            return Result.FromSuccess();
        }
        var name = gatewayEvent.Content.Split('\n')
            .First();
        name = name.Substring(0, name.Length < _options.MaxNameLength ? name.Length : _options.MaxNameLength);

        if (string.IsNullOrEmpty(name))
        {
            name = _options.DefaultName;
        }

        var channelResult = await _channelApi.StartThreadFromMessageAsync
        (
            gatewayEvent.ChannelID,
            gatewayEvent.ID,
            name,
            AutoArchiveDuration.Day,
            reason: "Auto thread creation",
            ct: ct
        );

        if (channelResult.IsSuccess)
        {
            _logger.LogInformation
                ("Created a thread in <@{Channel}> on message {Message}", gatewayEvent.ChannelID, gatewayEvent.ID);
        }

        return channelResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(channelResult);
    }
}