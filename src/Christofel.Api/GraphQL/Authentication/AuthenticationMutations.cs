using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.GraphQL.Common;
using HotChocolate.Types;

namespace Christofel.Api.GraphQL.Authentication
{
    [ExtendObjectType("Mutation")]
    public class AuthenticationMutations
    {
        public Task<RegisterDiscordPayload> RegisterDiscord(
            RegisterDiscordInput input,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new RegisterDiscordPayload(new UserError("Sorry, wrong input")));
        }
        
        public Task<RegisterCtuPayload> RegisterCtu(
            RegisterCtuInput input,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new RegisterCtuPayload(new UserError("Sorry, wrong input")));

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