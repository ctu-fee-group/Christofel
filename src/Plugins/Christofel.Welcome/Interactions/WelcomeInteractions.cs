//
//   WelcomeInteractions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Results;

namespace Christofel.Welcome.Interactions;

/// <summary>
/// Interaction handler for welcome buttons.
/// </summary>
[Ephemeral]
[Group("welcome")]
public class WelcomeInteractions : InteractionGroup
{
    private readonly IDiscordRestInteractionAPI _interactionApi;
    private readonly InteractionContext _context;
    private readonly ChristofelBaseContext _dbContext;
    private readonly UsersOptions _userOptions;
    private readonly ILogger<WelcomeInteractions> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly WelcomeOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WelcomeInteractions"/> class.
    /// </summary>
    /// <param name="interactionApi">The interaction api.</param>
    /// <param name="context">The context.</param>
    /// <param name="dbContext">The database context.</param>
    /// <param name="options">The options.</param>
    /// <param name="userOptions">The user options.</param>
    /// <param name="jsonOptions">The json options.</param>
    /// <param name="logger">The logger.</param>
    public WelcomeInteractions
    (
        IDiscordRestInteractionAPI interactionApi,
        InteractionContext context,
        ChristofelBaseContext dbContext,
        IOptionsSnapshot<WelcomeOptions> options,
        IOptionsSnapshot<UsersOptions> userOptions,
        IOptionsSnapshot<JsonSerializerOptions> jsonOptions,
        ILogger<WelcomeInteractions> logger
    )
    {
        _interactionApi = interactionApi;
        _context = context;
        _dbContext = dbContext;
        _userOptions = userOptions.Value;
        _logger = logger;
        _jsonOptions = jsonOptions.Get("Discord");
        _options = options.Value;

    }

    /// <summary>
    /// Send message with authentication link.
    /// </summary>
    /// <returns>A result that may have failed.</returns>
    [Button("auth")]
    public async Task<Result> HandleAuthButtonAsync()
    {
        // 1. Create user database entry with specified Discord account
        var dbUser = new DbUser
        {
            DiscordId = _context.User.ID,
            RegistrationCode = Guid.NewGuid().ToString()
        };

        _dbContext.Add(dbUser);
        try
        {
            await _dbContext.SaveChangesAsync(CancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError
            (
                e,
                $"Database context save changes has thrown an exception while saving user data (<@{_context.User.ID}>)"
            );
            return e;
        }

        // 2. Send message to the user with auth code.
        var link = _userOptions.AuthLink.Replace("{code}", dbUser.RegistrationCode);

        var messageResult = await _interactionApi.CreateFollowupMessageAsync
        (
            _context.ApplicationID,
            _context.Token,
            _options.AuthMessage.Replace("{Link}", link),
            ct: CancellationToken
        );

        return messageResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(messageResult);
    }

    /// <summary>
    /// Sends english welcome message.
    /// </summary>
    /// <returns>A result that may have failed.</returns>
    [Button("english")]
    public async Task<Result> HandleEnglishButtonAsync()
    {
        if (!File.Exists(_options.EnglishWelcomeEmbedFile))
        {
            return new NotFoundError("Could not find english welcome embed file.");
        }

        var embed = JsonSerializer.Deserialize<IEmbed>
            (await File.ReadAllTextAsync(_options.EnglishWelcomeEmbedFile, CancellationToken), _jsonOptions);
        if (embed is null)
        {
            // error
            return new GenericError("English welcome embed string could not be deserialized into an embed.");
        }

        var messageResult = await _interactionApi.CreateFollowupMessageAsync
        (
            _context.ApplicationID,
            _context.Token,
            embeds: new[] { embed },
            components: WelcomeMessageHelper.CreateWelcomeComponents(_options),
            ct: CancellationToken
        );

        return messageResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(messageResult);
    }
}