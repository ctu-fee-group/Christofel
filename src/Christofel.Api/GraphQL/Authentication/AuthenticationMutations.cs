using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Discord;
using Christofel.Api.GraphQL.Attributes;
using Christofel.Api.GraphQL.Common;
using Christofel.Api.OAuth;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Api.GraphQL.Authentication
{
    [ExtendObjectType("Mutation")]
    public class AuthenticationMutations
    {
        private readonly ILogger<AuthenticationMutations> _logger;
        private readonly BotOptions _botOptions;

        public AuthenticationMutations(
            ILogger<AuthenticationMutations> logger,
            IOptions<BotOptions> botOptions)
        {
            _botOptions = botOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Register using discord.
        /// This should be first step of registration.
        /// Second one is to register using CTU (registerCtu).
        /// </summary>
        /// <param name="input">Input of the mutation</param>
        /// <param name="dbContext">Db context to write user to</param>
        /// <param name="discordOauthHandler">handler for oauth2 token retrieval</param>
        /// <param name="discordApi"></param>
        /// <param name="guildApi"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [UseChristofelBaseDatabase]
        public async Task<RegisterDiscordPayload> RegisterDiscordAsync(
            RegisterDiscordInput input,
            [ScopedService] ChristofelBaseContext dbContext,
            [Service] DiscordOauthHandler discordOauthHandler,
            [Service] DiscordApi discordApi,
            [Service] IDiscordRestGuildAPI guildApi,
            CancellationToken cancellationToken)
        {
            OauthResponse response =
                await discordOauthHandler.ExchangeCodeAsync(input.OauthCode, input.RedirectUri, cancellationToken);

            if (response.IsError)
            {
                _logger.LogError($"There was an error while obtaining Discord token {response.ErrorResponse}");
                return new RegisterDiscordPayload(new UserError(response?.ErrorResponse?.ErrorDescription ??
                                                                "Unspecified error", UserErrorCode.OauthTokenRejected));
            }

            AuthorizedDiscordApi authDiscordApi = discordApi.GetAuthorizedApi(response.SuccessResponse?.AccessToken ??
                                                                              throw new InvalidOperationException(
                                                                                  "There was an error obtaining access token"));

            DiscordUser user = await authDiscordApi.GetMe();
            var memberResult = await guildApi.GetGuildMemberAsync(new Snowflake(_botOptions.GuildId),
                new Snowflake(user.Id), cancellationToken);
            if (!memberResult.IsSuccess)
            {
                if (memberResult.Error is NotFoundError)
                {
                    _logger.LogWarning(
                        $"User trying to register using Discord is not on the server ({user.Username}#{user.Discriminator})");
                    return new RegisterDiscordPayload(UserErrors.UserNotInGuild);
                }
                else
                {
                    _logger.LogError(
                        $"There was an error while getting the guild member ({user.Username}#{user.Discriminator}) from the rest api {memberResult.Error.Message}");
                    return new RegisterDiscordPayload(new UserError("Unspecified error", UserErrorCode.Unspecified));
                }
            }

            DbUser dbUser = new DbUser()
            {
                DiscordId = new Snowflake(user.Id),
                RegistrationCode = Guid.NewGuid().ToString()
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
        /// <param name="input">Input of the mutation</param>
        /// <param name="dbContext">Context with user information</param>
        /// <param name="ctuOauthHandler">Handler for obtaining access token</param>
        /// <param name="ctuAuthProcess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseChristofelBaseDatabase]
        public async Task<RegisterCtuPayload> RegisterCtuAsync(
            RegisterCtuInput input,
            [ScopedService] ChristofelBaseContext dbContext,
            [Service] CtuOauthHandler ctuOauthHandler,
            [Service] CtuAuthProcess ctuAuthProcess,
            [Service] IDiscordRestGuildAPI guildApi,
            CancellationToken cancellationToken)
        {
            DbUser? dbUser =
                await GetUserByRegistrationCode(input.RegistrationCode, dbContext.Users, cancellationToken);

            if (dbUser == null)
            {
                return new RegisterCtuPayload(UserErrors.InvalidRegistrationCode);
            }

            OauthResponse response =
                await ctuOauthHandler.ExchangeCodeAsync(input.OauthCode, input.RedirectUri, cancellationToken);

            if (response.IsError)
            {
                _logger.LogError($"There was an error while obtaining CTU token {response.ErrorResponse}");
                return new RegisterCtuPayload(new UserError(response?.ErrorResponse?.ErrorDescription ??
                                                            "Unspecified error", UserErrorCode.OauthTokenRejected));
            }

            if (response.SuccessResponse == null)
            {
                throw new InvalidOperationException("Could not obtain success response from oauth");
            }

            return await HandleRegistration(response.SuccessResponse.AccessToken, dbContext, dbUser, ctuOauthHandler,
                ctuAuthProcess, guildApi, cancellationToken);
        }

        /// <summary>
        /// Register using CTU using access token. If you want to use oauth2, use registerCtu mutation.
        /// This should be second and last step of registration.
        /// The first step is to register using Discord (registerDiscord).
        /// </summary>
        /// <param name="input">Input of the mutation</param>
        /// <param name="dbContext">Context with user information</param>
        /// <param name="ctuOauthHandler">Handler for obtaining access token</param>
        /// <param name="ctuAuthProcess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseChristofelBaseDatabase]
        public async Task<RegisterCtuPayload> RegisterCtuTokenAsync(
            RegisterCtuTokenInput input,
            [ScopedService] ChristofelBaseContext dbContext,
            [Service] CtuOauthHandler ctuOauthHandler,
            [Service] CtuAuthProcess ctuAuthProcess,
            [Service] IDiscordRestGuildAPI guildApi,
            CancellationToken cancellationToken)
        {
            DbUser? dbUser =
                await GetUserByRegistrationCode(input.RegistrationCode, dbContext.Users, cancellationToken);

            if (dbUser == null)
            {
                return new RegisterCtuPayload(UserErrors.InvalidRegistrationCode);
            }

            return await HandleRegistration(input.AccessToken, dbContext, dbUser, ctuOauthHandler, ctuAuthProcess,
                guildApi, cancellationToken);
        }

        /// <summary>
        /// Verify specified registration code to know what stage
        /// of registration should be used (registerDiscord or registerCtu)
        /// </summary>
        /// <param name="input">Input of the mutation</param>
        /// <param name="dbContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseReadOnlyChristofelBaseDatabase]
        public async Task<VerifyRegistrationCodePayload> VerifyRegistrationCodeAsync(
            VerifyRegistrationCodeInput input,
            [ScopedService] IReadableDbContext dbContext,
            CancellationToken cancellationToken)
        {
            DbUser? user =
                await GetUserByRegistrationCode(input.RegistrationCode, dbContext.Set<DbUser>(), cancellationToken);

            RegistrationCodeVerification verificationStage;
            if (user == null)
            {
                verificationStage = RegistrationCodeVerification.NotValid;
            }
            else if (user.AuthenticatedAt != null)
            {
                verificationStage = RegistrationCodeVerification.Done;
            }
            else if (user.CtuUsername != null)
            {
                verificationStage = RegistrationCodeVerification.CtuAuthorized;
            }
            else
            {
                verificationStage = RegistrationCodeVerification.DiscordAuthorized;
            }

            return new VerifyRegistrationCodePayload(verificationStage);
        }


        private async Task<DbUser?> GetUserByRegistrationCode(string registrationCode, IQueryable<DbUser> dbUsers,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(registrationCode))
            {
                return null;
            }

            DbUser? dbUser = await dbUsers
                .Where(x => x.AuthenticatedAt == null)
                .FirstOrDefaultAsync(x => x.RegistrationCode == registrationCode, cancellationToken);

            if (dbUser == null)
            {
                _logger.LogWarning(
                    $"User trying to register was not found in the database.");
            }

            if (dbUser?.AuthenticatedAt is not null)
            {
                _logger.LogWarning(
                    $"User trying to register was already registered. Aborting");
                dbUser = null;
            }

            return dbUser;
        }

        private async Task<RegisterCtuPayload> HandleRegistration(string accessToken, ChristofelBaseContext dbContext,
            DbUser dbUser, CtuOauthHandler ctuOauthHandler, CtuAuthProcess ctuAuthProcess,
            IDiscordRestGuildAPI guildApi,
            CancellationToken cancellationToken)
        {
            var memberResult = await guildApi.GetGuildMemberAsync(new Snowflake(_botOptions.GuildId),
                dbUser.DiscordId, cancellationToken);

            if (!memberResult.IsSuccess)
            {
                if (memberResult.Error is NotFoundError)
                {
                    _logger.LogWarning(
                        $"User trying to register using CTU is not on the server (discord id: {dbUser.DiscordId}, user id: {dbUser.UserId}).");
                    return new RegisterCtuPayload(UserErrors.UserNotInGuild);
                }
                else
                {
                    _logger.LogError(
                        $"There was an error while getting the guild member (<@{dbUser.DiscordId}> - {dbUser.UserId}) from the rest api {memberResult.Error.Message}");
                    return new RegisterCtuPayload(new UserError("Unspecified error", UserErrorCode.Unspecified));
                }
            }

            var user = memberResult.Entity.User;
            var username = user.HasValue ? (user.Value.Username + "#" + user.Value.Discriminator) : "Unknown username";
            using (_logger.BeginScope(
                $"CTU Registration of user ({username} - <@{dbUser.DiscordId}> - {dbUser.UserId})"))
            {
                try
                {
                    var authResult = await ctuAuthProcess.FinishAuthAsync(accessToken, ctuOauthHandler,
                        dbContext, _botOptions.GuildId,
                        dbUser, memberResult.Entity, cancellationToken);

                    if (!authResult.IsSuccess)
                    {
                        switch (authResult.Error)
                        {
                            case UserError userError:
                                _logger.LogWarning(
                                    "User error has occured during finalization of authentication of a user: {Error}",
                                    authResult.Error.Message);
                                
                                return new RegisterCtuPayload(userError);
                            case ExceptionError exceptionError:
                                _logger.LogError(exceptionError.Exception,
                                    "Could not register user using CTU, exception is not sent to the user");
                                
                                return new RegisterCtuPayload(new UserError("Unspecified error",
                                    UserErrorCode.Unspecified));
                            default:
                                return new RegisterCtuPayload(new UserError("Unspecified error",
                                    UserErrorCode.Unspecified));
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