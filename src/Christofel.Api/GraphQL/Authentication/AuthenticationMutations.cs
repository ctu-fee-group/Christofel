using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Discord;
using Christofel.Api.Exceptions;
using Christofel.Api.GraphQL.Attributes;
using Christofel.Api.GraphQL.Common;
using Christofel.Api.OAuth;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Api.GraphQL.Authentication
{
    [ExtendObjectType("Mutation")]
    public class AuthenticationMutations
    {
        private readonly ILogger<AuthenticationMutations> _logger;
        private readonly BotOptions _botOptions;
        private readonly DiscordSocketClient _botClient;

        public AuthenticationMutations(
            ILogger<AuthenticationMutations> logger,
            IOptions<BotOptions> botOptions,
            DiscordSocketClient botClient)
        {
            _botOptions = botOptions.Value;
            _logger = logger;
            _botClient = botClient;
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [UseChristofelBaseDatabase]
        public async Task<RegisterDiscordPayload> RegisterDiscordAsync(
            RegisterDiscordInput input,
            [ScopedService] ChristofelBaseContext dbContext,
            [Service] DiscordOauthHandler discordOauthHandler,
            [Service] DiscordApi discordApi,
            CancellationToken cancellationToken)
        {
            OauthResponse response =
                await discordOauthHandler.ExchangeCodeAsync(input.OauthCode, input.RedirectUri, cancellationToken);

            if (response.IsError)
            {
                _logger.LogError($"There was an error while obtaining Discord token {response.ErrorResponse}");
                return new RegisterDiscordPayload(new UserError(response?.ErrorResponse?.ErrorDescription ??
                                                                "Unspecified error"));
            }

            AuthorizedDiscordApi authDiscordApi = discordApi.GetAuthorizedApi(response.SuccessResponse?.AccessToken ??
                                                                              throw new InvalidOperationException(
                                                                                  "There was an error obtaining access token"));

            DiscordUser user = await authDiscordApi.GetMe();
            RestGuildUser? guildUser = await _botClient.Rest.GetGuildUserAsync(_botOptions.GuildId, user.Id,
                new RequestOptions() { CancelToken = cancellationToken });
            if (guildUser == null)
            {
                _logger.LogWarning(
                    $"User trying to register using Discord is not on the server ({user.Username}#{user.Discriminator})");
                return new RegisterDiscordPayload(
                    new UserError(
                        $"User you are trying to log in with ({user.Username}#{user.Discriminator}) is not on the Discord server. Are you sure you are logging in with the correct user?"));
            }

            DbUser dbUser = new DbUser()
            {
                DiscordId = user.Id,
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseChristofelBaseDatabase]
        public async Task<RegisterCtuPayload> RegisterCtuAsync(
            RegisterCtuInput input,
            [ScopedService] ChristofelBaseContext dbContext,
            [Service] CtuOauthHandler ctuOauthHandler,
            [Service] CtuAuthProcess ctuAuthProcess,
            CancellationToken cancellationToken)
        {
            DbUser? dbUser = await dbContext.Users
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.RegistrationCode == input.RegistrationCode, cancellationToken);

            if (dbUser == null)
            {
                _logger.LogWarning(
                    $"User trying to register was not found in the database.");
                return new RegisterCtuPayload(
                    new UserError(
                        $"Specified registration code is not valid"));
            }

            RestGuildUser? guildUser = await _botClient.Rest.GetGuildUserAsync(_botOptions.GuildId, dbUser.DiscordId,
                new RequestOptions() { CancelToken = cancellationToken });

            if (guildUser == null)
            {
                _logger.LogWarning(
                    $"User trying to register using CTU is not on the server (discord id: {dbUser.DiscordId}, user id: {dbUser.UserId}).");
                return new RegisterCtuPayload(
                    new UserError(
                        $"User you are trying to log in is not on the Discord server. Are you sure you are logging in with the correct user?"));
            }

            OauthResponse response =
                await ctuOauthHandler.ExchangeCodeAsync(input.OauthCode, input.RedirectUri, cancellationToken);

            if (response.IsError)
            {
                _logger.LogError($"There was an error while obtaining CTU token {response.ErrorResponse}");
                return new RegisterCtuPayload(new UserError(response?.ErrorResponse?.ErrorDescription ??
                                                            "Unspecified error"));
            }

            if (response.SuccessResponse == null)
            {
                throw new InvalidOperationException("Could not obtain success response from oauth");
            }

            using (_logger.BeginScope($"CTU Registration of user ({guildUser} - {dbUser.DiscordId} - {dbUser.UserId})"))
            {
                try
                {
                    await ctuAuthProcess.FinishAuthAsync(response.SuccessResponse.AccessToken, ctuOauthHandler,
                        dbContext,
                        dbUser, guildUser);
                    return new RegisterCtuPayload(dbUser);
                }
                catch (UserException e)
                {
                    _logger.LogError(e, "Could not register user using CTU, sending him the error");
                    return new RegisterCtuPayload(new UserError(e.Message));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not register user using CTU, exception is not sent to the user");
                    return new RegisterCtuPayload(new UserError("Unspecified error"));
                }
            }
        }

        /// <summary>
        /// Verify specified registration code to know what stage
        /// of registration should be used (registerDiscord or registerCtu)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        [UseReadOnlyChristofelBaseDatabase]
        public async Task<VerifyRegistrationCodePayload> VerifyRegistrationCodeAsync(
            VerifyRegistrationCodeInput input,
            [ScopedService] IReadableDbContext dbContext)
        {
            DbUser? user = await dbContext.Set<DbUser>()
                .FirstOrDefaultAsync(x => x.RegistrationCode == input.RegistrationCode);

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
    }
}