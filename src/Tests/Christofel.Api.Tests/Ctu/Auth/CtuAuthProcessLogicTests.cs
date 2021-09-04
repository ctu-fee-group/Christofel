using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Ctu.Auth.Steps;
using Christofel.Api.Ctu.Auth.Tasks;
using Christofel.Api.Ctu.Extensions;
using Christofel.Api.GraphQL.Types;
using Christofel.Api.OAuth;
using Christofel.Api.Tests.Data.Ctu.Auth;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.User;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using TestSupport.EfHelpers;
using Xunit;

namespace Christofel.Api.Tests.Ctu.Auth
{
    public partial class CtuAuthProcessLogicTests : IDisposable
    {
        protected readonly ChristofelBaseContext _dbContext;
        protected readonly IDisposable _optionsDisposable;

        protected readonly string _dummyAccessToken = "myToken";
        protected readonly string _dummyUsername = "someUsername";
        protected readonly ulong _dummyGuildId = 93249823482348;

        public CtuAuthProcessLogicTests()
        {
            var options = SqliteInMemory.CreateOptions<ChristofelBaseContext>();
            _optionsDisposable = options;

            _dbContext = new ChristofelBaseContext(options);
            _dbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _optionsDisposable?.Dispose();
        }

        protected IGuildMember CreateDummyGuildMember(DbUser user)
        {
            return GuildMemberRepository.CreateDummyGuildMember(user);
        }

        protected Mock<ICtuTokenApi> GetMockedTokenApi(DbUser user)
        {
            return OauthTokenApiRepository.GetMockedTokenApi(user, _dummyUsername);
        }
        
        [Fact]
        public async Task CallsCondition()
        {
            var mockCondition = new Mock<IPreAuthCondition>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IPreAuthCondition>(p => mockCondition.Object)
                .AddAuthCondition<IPreAuthCondition>()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync(_dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember);

            mockCondition.Verify(service => service.CheckPreAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CallsTask()
        {
            var mockTask = new Mock<IAuthTask>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IAuthTask>(p => mockTask.Object)
                .AddAuthTask<IAuthTask>()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync(_dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember);

            mockTask.Verify(service => service.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CallsStep()
        {
            var mockStep = new Mock<IAuthStep>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IAuthStep>(p => mockStep.Object)
                .AddAuthStep<IAuthStep>()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync(_dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember);

            mockStep.Verify(service => service.FillDataAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}