using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Christofel.Api.Tests.Ctu.Auth
{
    public class CtuAuthProcessConditionUsernameMatchesTests : CtuAuthProcessConditionTests<CtuUsernameMatchesCondition>
    {
        [Fact]
        public async Task DoesNotAllowNonMatchingFilledUsername()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync("non matching username");
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, "real username");

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync(_dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task AllowsMatchingUsername()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync(_dummyUsername);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);
            
            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, _dummyUsername);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync(_dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember);

            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public async Task AllowsNonFilledUsername()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, _dummyUsername);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync(_dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember);

            Assert.True(result.IsSuccess);
        }
    }
}