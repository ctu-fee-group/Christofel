//
//   AuthenticationMutations.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Discord;
using Christofel.Api.GraphQL.Attributes;
using Christofel.Api.GraphQL.Common;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Extensions;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.CtuAuth;
using Christofel.CtuAuth.Auth;
using Christofel.CtuAuth.Errors;
using Christofel.OAuth;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Abstractions.Results;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Rest.Results;
using Remora.Results;

namespace Christofel.Api.GraphQL.Authentication
{
    /// <summary>
    /// Mutations for user registration.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class AuthenticationMutations
    {
        private readonly BotOptions _botOptions;
        private readonly ILogger<AuthenticationMutations> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMutations"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="botOptions">The options of the bot.</param>
        public AuthenticationMutations
        (
            ILogger<AuthenticationMutations> logger,
            IOptions<BotOptions> botOptions
        )
        {
            _botOptions = botOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Register using discord.
        /// This should be first step of registration.
        /// Second one is to register using CTU (registerCtu).
        /// </summary>
        /// <param name="input">The input of the mutation.</param>
        /// <param name="dbContext">The database context to write user to.</param>
        /// <param name="discordOauthHandler">The handler for oauth2 token retrieval.</param>
        /// <param name="discordApi">The discord api.</param>
        /// <param name="guildApi">The guild api.</param>
        /// <param name="cancellationToken">The cancellation token for the opration.</param>
        /// <returns>Payload to the user.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the user token could not be obtained.</exception>
        [UseChristofelBaseDatabase]
        public async Task<RegisterDiscordPayload> RegisterDiscordAsync
        (
            RegisterDiscordInput input,
            [ScopedService] ChristofelBaseContext dbContext,
            [Service] DiscordOauthHandler discordOauthHandler,
            [Service] DiscordApi discordApi,
            [Service] IDiscordRestGuildAPI guildApi,
            CancellationToken cancellationToken
        )
        {
            OauthResponse response =
                await discordOauthHandler.GrantAuthorizationCodeAsync(input.OauthCode, input.RedirectUri, cancellationToken);

            if (response.IsError)
            {
                _logger.LogError($"There was an error while obtaining Discord token {response.ErrorResponse}");
                return new RegisterDiscordPayload
                (
                    new UserError
                    (
                        response?.ErrorResponse?.ErrorDescription ??
                        "Unspecified error",
                        UserErrorCode.OauthTokenRejected
                    )
                );
            }

            AuthorizedDiscordApi authDiscordApi = discordApi.GetAuthorizedApi
            (
                response.SuccessResponse?.AccessToken ??
                throw new InvalidOperationException("There was an error obtaining access token")
            );

            DiscordUser user = await authDiscordApi.GetMe();
            var memberResult = await guildApi.GetGuildMemberAsync
            (
                new Snowflake(_botOptions.GuildId, Constants.DiscordEpoch),
                new Snowflake(user.Id, Constants.DiscordEpoch),
                cancellationToken
            );
            if (!memberResult.IsSuccess)
            {
                if (memberResult.Error is NotFoundError
                    || (memberResult.Error as RestResultError<RestError>)?.Error.Code == DiscordError.UnknownMember)
                {
                    _logger.LogWarning
                    (
                        $"User trying to register using Discord is not on the server ({user.Username}#{user.Discriminator})"
                    );
                    return new RegisterDiscordPayload(UserErrors.UserNotInGuild);
                }

                _logger.LogResultError
                (
                    memberResult,
                    $"There was an error while getting the guild member ({user.Username}#{user.Discriminator}) from the rest api"
                );
                return new RegisterDiscordPayload(new UserError("Unspecified error", UserErrorCode.Unspecified));
            }

            DbUser dbUser = new DbUser
            {
                DiscordId = new Snowflake(user.Id, Constants.DiscordEpoch),
                RegistrationCode = Guid.NewGuid().ToString(),
            };

            dbContext.Add(dbUser);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new RegisterDiscordPayload(dbUser, dbUser.RegistrationCode);
        }

        /// <summary>
        /// Register using CTU.
        /// This should be second and last step of registration.
        /// The first step is to register using Discord (registerDiscord).
        /// </summary>
        /// <param name="input">Input of the mutation.</param>
        /// <param name="dbContext">The database context with user information.</param>
        /// <param name="ctuOauthHandler">The handler for obtaining access token.</param>
        /// <param name="ctuAuthProcess">The ctu auth process handler.</param>
        /// <param name="guildApi">The guild api.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>Payload for the user.</returns>
        [UseChristofelBaseDatabase]
        public async Task<RegisterCtuPayload> RegisterCtuFelAsync
        (
            RegisterCtuInput input,
            [ScopedService] ChristofelBaseContext dbContext,
            [Service] CtuOauthHandler ctuOauthHandler,
            [Service] CtuAuthProcess ctuAuthProcess,
            [Service] IDiscordRestGuildAPI guildApi,
            CancellationToken cancellationToken
        )
        {
            var dbUser =
                await GetUserByRegistrationCode(input.RegistrationCode, dbContext.Users, cancellationToken);

            if (dbUser == null)
            {
                return new RegisterCtuPayload(UserErrors.InvalidRegistrationCode);
            }

            OauthResponse response =
                await ctuOauthHandler.GrantAuthorizationCodeAsync(input.OauthCode, input.RedirectUri, cancellationToken);

            if (response.IsError)
            {
                _logger.LogError($"There was an error while obtaining CTU token {response.ErrorResponse}");
                return new RegisterCtuPayload
                (
                    new UserError
                    (
                        response?.ErrorResponse?.ErrorDescription ??
                        "Unspecified error",
                        UserErrorCode.OauthTokenRejected
                    )
                );
            }

            if (response.SuccessResponse == null)
            {
                throw new InvalidOperationException("Could not obtain success response from oauth");
            }

            return await HandleRegistration
            (
                response.SuccessResponse.AccessToken,
                dbContext,
                dbUser,
                ctuOauthHandler,
                ctuAuthProcess,
                guildApi,
                cancellationToken
            );
        }

        /// <summary>
        /// Register using CTU using access token. If you want to use oauth2, use registerCtu mutation.
        /// This should be second and last step of registration.
        /// The first step is to register using Discord (registerDiscord).
        /// </summary>
        /// <param name="input">Input of the mutation.</param>
        /// <param name="dbContext">The database context with user information.</param>
        /// <param name="ctuOauthHandler">The handler for obtaining access token.</param>
        /// <param name="ctuAuthProcess">The ctu auth process handler.</param>
        /// <param name="guildApi">The guild api.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>Payload for the user.</returns>
        [UseChristofelBaseDatabase]
        public async Task<RegisterCtuPayload> RegisterCtuTokenAsync
        (
            RegisterCtuTokenInput input,
            [ScopedService] ChristofelBaseContext dbContext,
            [Service] CtuOauthHandler ctuOauthHandler,
            [Service] CtuAuthProcess ctuAuthProcess,
            [Service] IDiscordRestGuildAPI guildApi,
            CancellationToken cancellationToken
        )
        {
            var dbUser =
                await GetUserByRegistrationCode(input.RegistrationCode, dbContext.Users, cancellationToken);

            if (dbUser == null)
            {
                return new RegisterCtuPayload(UserErrors.InvalidRegistrationCode);
            }

            return await HandleRegistration
            (
                input.AccessToken,
                dbContext,
                dbUser,
                ctuOauthHandler,
                ctuAuthProcess,
                guildApi,
                cancellationToken
            );
        }

        private async Task<DbUser?> GetUserByRegistrationCode
        (
            string registrationCode,
            IQueryable<DbUser> dbUsers,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(registrationCode))
            {
                return null;
            }

            var dbUser = await dbUsers
                .Where(x => x.AuthenticatedAt == null)
                .FirstOrDefaultAsync(x => x.RegistrationCode == registrationCode, cancellationToken);

            if (dbUser == null)
            {
                _logger.LogWarning("User trying to register was not found in the database.");
            }

            if (dbUser?.AuthenticatedAt is not null)
            {
                _logger.LogWarning("User trying to register was already registered. Aborting");
                dbUser = null;
            }

            return dbUser;
        }

        private async Task<RegisterCtuPayload> HandleRegistration
        (
            string accessToken,
            ChristofelBaseContext dbContext,
            DbUser dbUser,
            CtuOauthHandler ctuOauthHandler,
            CtuAuthProcess ctuAuthProcess,
            IDiscordRestGuildAPI guildApi,
            CancellationToken cancellationToken
        )
        {
            var memberResult = await guildApi.GetGuildMemberAsync
            (
                new Snowflake(_botOptions.GuildId, Constants.DiscordEpoch),
                dbUser.DiscordId,
                cancellationToken
            );

            if (!memberResult.IsSuccess)
            {
                if (memberResult.Error is NotFoundError
                    || (memberResult.Error as RestResultError<RestError>)?.Error.Code == DiscordError.UnknownMember)
                {
                    _logger.LogWarning
                    (
                        $"User trying to register using CTU is not on the server (discord id: {dbUser.DiscordId}, user id: {dbUser.UserId})."
                    );
                    return new RegisterCtuPayload(UserErrors.UserNotInGuild);
                }

                _logger.LogResultError
                (
                    memberResult,
                    $"There was an error while getting the guild member (<@{dbUser.DiscordId}> - {dbUser.UserId}) from the rest api."
                );
                return new RegisterCtuPayload(new UserError("Unspecified error", UserErrorCode.Unspecified));
            }

            var user = memberResult.Entity.User;
            var username = user.HasValue
                ? user.Value.Username + "#" + user.Value.Discriminator
                : "Unknown username";
            using (_logger.BeginScope
                ($"CTU Registration of user ({username} - <@{dbUser.DiscordId}> - {dbUser.UserId})"))
            {
                try
                {
                    var authResult = await ctuAuthProcess.FinishAuthAsync
                    (
                        accessToken,
                        ctuOauthHandler,
                        dbContext,
                        _botOptions.GuildId,
                        dbUser,
                        memberResult.Entity,
                        cancellationToken
                    );

                    if (!authResult.IsSuccess)
                    {
                        var error = authResult.Error;

                        if (error is DuplicateError)
                        {
                            error = UserErrors.RejectedDuplicateUser;
                        }

                        switch (error)
                        {
                            case UserError userError:
                                _logger.LogResultError
                                (
                                    authResult,
                                    "User error has occured during finalization of authentication of a user"
                                );

                                return new RegisterCtuPayload(userError);
                            case ExceptionError exceptionError:
                                _logger.LogError
                                (
                                    exceptionError.Exception,
                                    "Could not register user using CTU, exception is not sent to the user"
                                );

                                return new RegisterCtuPayload
                                (
                                    new UserError
                                    (
                                        "Unspecified error",
                                        UserErrorCode.Unspecified
                                    )
                                );
                            case SoftAuthError softError:
                                _logger.LogResultError
                                (
                                    authResult,
                                    "User error has occured during task stage of finalization of authentication of a user"
                                );

                                return new RegisterCtuPayload(UserErrors.SoftAuthError);
                            default:
                                _logger.LogResultError
                                (
                                    authResult,
                                    "There was an error in authentication process that prevented the user from registration."
                                );
                                return new RegisterCtuPayload
                                (
                                    new UserError
                                    (
                                        "Unspecified error",
                                        UserErrorCode.Unspecified
                                    )
                                );
                        }
                    }

                    return new RegisterCtuPayload(dbUser);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not register user using CTU, exception is not sent to the user");
                    return new RegisterCtuPayload(new UserError("Unspecified error", UserErrorCode.Unspecified));
                }
            }
        }
    }
}
