//
//   WelcomeInteractions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Results;

namespace Christofel.Welcome.Interactions;

/// <summary>
/// Interaction handler for welcome buttons.
/// </summary>
public class WelcomeInteractions
{
    private readonly IDiscordRestInteractionAPI _interactionApi;
    private readonly IInteractionContext _context;
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
        IInteractionContext context,
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
    /// <param name="language">The language to send the auth in.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result that may have failed.</returns>
    public async Task<Result> HandleAuthButtonAsync(string language, CancellationToken ct)
    {
        if (!_context.TryGetUserID(out var userId))
        {
            return new GenericError("Could not get user id from context.");
        }

        var dbUser = await _dbContext
            .Users
            .FirstOrDefaultAsync
            (
                x => x.AuthenticatedAt == null && x.DiscordId == userId.Value && x.RegistrationCode != null,
                ct
            );
        if (dbUser is null)
        {
            dbUser = new DbUser
            {
                DiscordId = userId.Value,
                RegistrationCode = Guid.NewGuid().ToString(),
                InteractionToken = _context.Interaction.Token,
            };

            _dbContext.Add(dbUser);
            try
            {
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                _logger.LogError
                (
                    e,
                    $"Database context save changes has thrown an exception while saving user data (<@{userId.Value}>)"
                );
                return e;
            }
        }
        else
        {
            dbUser.InteractionToken = _context.Interaction.Token;
        }

        // 2. Send message to the user with auth code.
        var link = _userOptions.AuthLink.Replace("{code}", dbUser.RegistrationCode);

        var components = new IMessageComponent[]
        {
            new ActionRowComponent
            (
                new[]
                {
                    new ButtonComponent
                    (
                        ButtonComponentStyle.Link,
                        _options.Translations[language].AuthButtonLabel,
                        new PartialEmoji(Name: _options.AuthButtonEmoji),
                        URL: link
                    )
                }
            )
        };

        if (!Uri.TryCreate(link, UriKind.Absolute, out _))
        {
            components = new IMessageComponent[] { };
        }

        var messageResult = await _interactionApi.CreateFollowupMessageAsync
        (
            _context.Interaction.ApplicationID,
            _context.Interaction.Token,
            _options.Translations[language].AuthMessage.Replace("{Link}", link),
            flags: MessageFlags.Ephemeral,
            components: components,
            ct: ct
        );

        return messageResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(messageResult);
    }

    /// <summary>
    /// Sends english welcome message.
    /// </summary>
    /// <param name="language">The language to show.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result that may have failed.</returns>
    public async Task<Result> HandleShowAsync(string language, CancellationToken ct)
    {
        if (!_options.Translations.ContainsKey(language))
        {
            return new NotFoundError($"Could not find {language} translation for welcome message.");
        }

        var translation = _options.Translations[language];
        if (!File.Exists(translation.EmbedFilePath))
        {
            return new NotFoundError("Could not find english welcome embed file.");
        }

        var embed = JsonSerializer.Deserialize<IEmbed>
            (await File.ReadAllTextAsync(translation.EmbedFilePath, ct), _jsonOptions);
        if (embed is null)
        {
            // error
            return new GenericError("English welcome embed string could not be deserialized into an embed.");
        }

        var messageResult = await _interactionApi.CreateFollowupMessageAsync
        (
            _context.Interaction.ApplicationID,
            _context.Interaction.Token,
            embeds: new[] { embed },
            components: WelcomeMessageHelper.CreateWelcomeComponents(_options, language),
            flags: MessageFlags.Ephemeral,
            ct: ct
        );

        return messageResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(messageResult);
    }
}