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
        
        public Task<CtuRegisterPayload> RegisterCtu(
            RegisterCtuInput input,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new CtuRegisterPayload(new UserError("Sorry, wrong input")));
        }
    }
}