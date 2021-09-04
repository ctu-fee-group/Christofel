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
            var mockCondition = new Mock<ConditionRepository.MockCondition>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IPreAuthCondition, ConditionRepository.MockCondition>(p => mockCondition.Object)
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
            var mockTask = new Mock<TaskRepository.MockTask>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
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
        }

        [Fact]
        public async Task CallsStep()
        {
            var mockStep = new Mock<StepRepository.MockStep>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IAuthStep, StepRepository.MockStep>(p => mockStep.Object)
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

        [Fact]
        public async Task CallsMultipleConditions()
        {
            var mockCondition = new Mock<ConditionRepository.MockCondition>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.SuccessfulCondition>()
                .AddScoped<IPreAuthCondition, ConditionRepository.MockCondition>(p => mockCondition.Object)
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
            
            mockCondition = new Mock<ConditionRepository.MockCondition>();
            
            services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IPreAuthCondition, ConditionRepository.MockCondition>(p => mockCondition.Object)
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();
            
            dummyGuildMember = CreateDummyGuildMember(user);
            successfulOauthHandler = GetMockedTokenApi(user);

            process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync(_dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember);

            mockCondition.Verify(service => service.CheckPreAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CallsMultipleTasks()
        {
            var mockTask = new Mock<TaskRepository.MockTask>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthTask<TaskRepository.SuccessfulTask>()
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
                .AddAuthTask<TaskRepository.SuccessfulTask>()
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
        public async Task CallsMultipleSteps()
        {
            var mockStep = new Mock<StepRepository.MockStep>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthStep<StepRepository.SuccessfulStep>()
                .AddScoped<IAuthStep, StepRepository.MockStep>(p => mockStep.Object)
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
            
            mockStep = new Mock<StepRepository.MockStep>();
            
            services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IAuthStep, StepRepository.MockStep>(p => mockStep.Object)
                .AddAuthStep<StepRepository.SuccessfulStep>()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();
            
            dummyGuildMember = CreateDummyGuildMember(user);
            successfulOauthHandler = GetMockedTokenApi(user);

            process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync(_dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember);

            mockStep.Verify(service => service.FillDataAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}