using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.GraphQL.Common;
using HotChocolate.Types;

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
        
        public Task<RegisterCtuPayload> RegisterCtu(
            RegisterCtuInput input,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new RegisterCtuPayload(new UserError("Sorry, wrong input")));

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