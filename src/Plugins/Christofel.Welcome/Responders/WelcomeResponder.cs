//
//  WelcomeResponder.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Configuration;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Christofel.Welcome.Responders;

/// <summary>
/// Responds to guild member add event.
/// </summary>
public class WelcomeResponder : IResponder<IGuildMemberAdd>
{
    private readonly WelcomeMessage _welcomeMessage;
    private readonly IDiscordRestUserAPI _userApi;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly BotOptions _botOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="WelcomeResponder"/> class.
    /// </summary>
    /// <param name="welcomeMessage">The welcome message service.</param>
    /// <param name="userApi">The discord rest user api..</param>
    /// <param name="channelApi">The discord rest channel api.</param>
    /// <param name="botOptions">The bot options.</param>
    /// <param name="options">The welcome message options.</param>
    public WelcomeResponder
    (
        WelcomeMessage welcomeMessage,
        IDiscordRestUserAPI userApi,
        IDiscordRestChannelAPI channelApi,
        IOptions<BotOptions> botOptions
    )
    {
        _welcomeMessage = welcomeMessage;
        _userApi = userApi;
        _channelApi = channelApi;
        _botOptions = botOptions.Value;
    }

    /// <inheritdoc />
    public async Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.GuildID.Value != _botOptions.GuildId)
        {
            return Result.FromSuccess();
        }

        if (!gatewayEvent.User.IsDefined(out var user))
        {
            return Result.FromSuccess();
        }

        var dmChannelResult = await _userApi.CreateDMAsync(user.ID, ct);

        if (!dmChannelResult.IsDefined(out var dmChannel))
        {
            return Result.FromError
            (
                new GenericError("Cannot create a dm channel for sending message to new member."),
                dmChannelResult
            );
        }

        var messageResult = await _welcomeMessage.SendWelcomeMessage(dmChannel.ID, default, ct);

        return messageResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(messageResult);
    }
}