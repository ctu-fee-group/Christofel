using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Tasks;
using Christofel.Api.Ctu.Extensions;
using Christofel.Api.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Christofel.Api.Tests.Ctu.Auth
{
    public class CtuAuthProcessLogicDataTests : CtuAuthProcessLogicTests
    {
        [Fact]
        public async Task CtuUsernameIsNotSetIfFilled()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.FailingCondition>()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            string alreadySetUsername = "already set username";
            user.CtuUsername = alreadySetUsername;
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync(_dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember);

            Assert.Matches(alreadySetUsername, user.CtuUsername);
        }

        [Fact]
        public async Task FailedTaskFinishesAllTasks()
        {
            var mockTask = new Mock<TaskRepository.MockTask>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthTask<TaskRepository.FailingTask>()
                .AddScoped<IAuthTask, TaskRepository.MockTask>(p => mockTask.Object)
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

            mockTask = new Mock<TaskRepository.MockTask>();

            services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IAuthTask, TaskRepository.MockTask>(p => mockTask.Object)
                .AddAuthTask<TaskRepository.FailingTask>()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            dummyGuildMember = CreateDummyGuildMember(user);
            successfulOauthHandler = GetMockedTokenApi(user);

            process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync(_dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember);

            mockTask.Verify(service => service.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CtuUsernameIsSetAfterFailedConditions()
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.FailingCondition>()
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

            Assert.NotNull(user.CtuUsername);
            Assert.Matches(_dummyUsername, user.CtuUsername);
        }

        [Fact]
        public async Task ContextIsSavedAfterFailedConditions()
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.FailingCondition>()
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

            _dbContext.ChangeTracker.Clear();

            var savedUser = _dbContext.Users.First();
            Assert.NotNull(savedUser.CtuUsername);
            Assert.Matches(_dummyUsername, savedUser.CtuUsername);
        }

        [Fact]
        public async Task CtuUsernameIsSetAfterSuccessfulConditions()
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.SuccessfulCondition>()
                .AddAuthStep<StepRepository.FailingStep>()
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

            Assert.NotNull(user.CtuUsername);
            Assert.Matches(_dummyUsername, user.CtuUsername);
        }

        [Fact]
        public async Task ContextIsSavedAfterSuccessfulConditions()
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.SuccessfulCondition>()
                .AddAuthStep<StepRepository.FailingStep>()
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

            _dbContext.ChangeTracker.Clear();

            var savedUser = _dbContext.Users.First();
            Assert.NotNull(savedUser.CtuUsername);
            Assert.Matches(_dummyUsername, savedUser.CtuUsername);
        }

        [Fact]
        public async Task ContextIsSavedAfterSuccessfulSteps()
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthStep<StepRepository.SetAuthenticatedAtStep>()
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

            _dbContext.ChangeTracker.Clear();

            var savedUser = _dbContext.Users.First();
            Assert.NotNull(savedUser.AuthenticatedAt);
        }
    }
}