using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Extensions;
using Christofel.Api.OAuth;
using Christofel.Api.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Christofel.Api.Tests.Ctu.Auth
{
    public class CtuAuthProcessLogicTokenApiTests : CtuAuthProcessLogicTests
    {
        [Fact]
        public async Task FailedUsernameRetrievalReturnsError()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);

            var process = services.GetRequiredService<CtuAuthProcess>();

            var failingOauthHandler = new Mock<ICtuTokenApi>();
            failingOauthHandler
                .Setup(tokenApi => tokenApi.CheckTokenAsync(_dummyAccessToken, It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>();

            var result =
                await process.FinishAuthAsync(_dummyAccessToken, failingOauthHandler.Object, _dbContext, _dummyGuildId,
                    user, dummyGuildMember);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task SuccessfulUsernameRetrievalReturnsSuccess()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result =
                await process.FinishAuthAsync(_dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                    _dummyGuildId,
                    user, dummyGuildMember);

            Assert.True(result.IsSuccess);
        }
    }
}