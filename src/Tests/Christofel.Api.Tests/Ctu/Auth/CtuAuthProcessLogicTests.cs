//
//   CtuAuthProcessLogicTests.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Ctu.Auth.Steps;
using Christofel.Api.Ctu.Auth.Tasks;
using Christofel.Api.Ctu.Extensions;
using Christofel.Api.Tests.Data.Ctu.Auth;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Remora.Discord.API.Abstractions.Objects;
using TestSupport.EfHelpers;
using Xunit;

namespace Christofel.Api.Tests.Ctu.Auth
{
    /// <summary>
    /// Tests logic of ctu auth process using mock, custom data.
    /// </summary>
    public class CtuAuthProcessLogicTests : IDisposable
    {
        /// <summary>
        /// Gets the database context.
        /// </summary>
        protected ChristofelBaseContext DbContext { get; }

        /// <summary>
        /// Gets dummy access token used for testing.
        /// </summary>
        protected string DummyAccessToken => "myToken";

        /// <summary>
        /// Gets dummy guild id used for testing.
        /// </summary>
        protected ulong DummyGuildId => 93249823482348;

        /// <summary>
        /// Gets dummy username used for testing.
        /// </summary>
        protected string DummyUsername => "someUsername";

        /// <summary>
        /// Gets options of the database context.
        /// </summary>
        protected DbContextOptionsDisposable<ChristofelBaseContext> OptionsDisposable { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuAuthProcessLogicTests"/> class.
        /// </summary>
        public CtuAuthProcessLogicTests()
        {
            var options = SqliteInMemory.CreateOptions<ChristofelBaseContext>();
            OptionsDisposable = options;

            DbContext = new ChristofelBaseContext(options);
            DbContext.Database.EnsureCreated();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            DbContext?.Dispose();
            OptionsDisposable?.Dispose();
        }

        /// <summary>
        /// Creates dummy guild member with the given user.
        /// </summary>
        /// <param name="user">The user that the guild member should represent.</param>
        /// <returns>Guild member that represents the user.</returns>
        protected IGuildMember CreateDummyGuildMember(DbUser user) => GuildMemberRepository.CreateDummyGuildMember
            (user);

        /// <summary>
        /// Creates mocked <see cref="ICtuTokenApi"/> that will return the given user.
        /// </summary>
        /// <param name="user">The user that should be returned.</param>
        /// <returns>Mocked <see cref="ICtuTokenApi"/> that will return the given user.</returns>
        protected Mock<ICtuTokenApi> GetMockedTokenApi(DbUser user) => OauthTokenApiRepository.GetMockedTokenApi
            (user, DummyUsername);

        /// <summary>
        /// Tests that the process calls condition method.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task CallsCondition()
        {
            var mockCondition = new Mock<ConditionRepository.MockCondition>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IPreAuthCondition, ConditionRepository.MockCondition>(p => mockCondition.Object)
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync
            (
                DummyAccessToken,
                successfulOauthHandler.Object,
                DbContext,
                DummyGuildId,
                user,
                dummyGuildMember
            );

            mockCondition.Verify
            (
                service => service.CheckPreAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        /// <summary>
        /// Tests that the process calls task method.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task CallsTask()
        {
            var mockTask = new Mock<TaskRepository.MockTask>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IAuthTask, TaskRepository.MockTask>(p => mockTask.Object)
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync
            (
                DummyAccessToken,
                successfulOauthHandler.Object,
                DbContext,
                DummyGuildId,
                user,
                dummyGuildMember
            );

            mockTask.Verify
            (
                service => service.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        /// <summary>
        /// Tests that the process calls step method.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task CallsStep()
        {
            var mockStep = new Mock<StepRepository.MockStep>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IAuthStep, StepRepository.MockStep>(p => mockStep.Object)
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync
            (
                DummyAccessToken,
                successfulOauthHandler.Object,
                DbContext,
                DummyGuildId,
                user,
                dummyGuildMember
            );

            mockStep.Verify
            (
                service => service.FillDataAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        /// <summary>
        /// Tests that the process calls all of the conditions methods.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
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

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync
            (
                DummyAccessToken,
                successfulOauthHandler.Object,
                DbContext,
                DummyGuildId,
                user,
                dummyGuildMember
            );

            mockCondition.Verify
            (
                service => service.CheckPreAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );

            mockCondition = new Mock<ConditionRepository.MockCondition>();

            services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddScoped<IPreAuthCondition, ConditionRepository.MockCondition>(p => mockCondition.Object)
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            dummyGuildMember = CreateDummyGuildMember(user);
            successfulOauthHandler = GetMockedTokenApi(user);

            process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync
            (
                DummyAccessToken,
                successfulOauthHandler.Object,
                DbContext,
                DummyGuildId,
                user,
                dummyGuildMember
            );

            mockCondition.Verify
            (
                service => service.CheckPreAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        /// <summary>
        /// Tests that the process calls all of the tasks methods.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
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

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync
            (
                DummyAccessToken,
                successfulOauthHandler.Object,
                DbContext,
                DummyGuildId,
                user,
                dummyGuildMember
            );

            mockTask.Verify
            (
                service => service.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );

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
            await process.FinishAuthAsync
            (
                DummyAccessToken,
                successfulOauthHandler.Object,
                DbContext,
                DummyGuildId,
                user,
                dummyGuildMember
            );

            mockTask.Verify
            (
                service => service.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        /// <summary>
        /// Tests that the process calls all of the steps methods.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
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

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync
            (
                DummyAccessToken,
                successfulOauthHandler.Object,
                DbContext,
                DummyGuildId,
                user,
                dummyGuildMember
            );

            mockStep.Verify
            (
                service => service.FillDataAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );

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
            await process.FinishAuthAsync
            (
                DummyAccessToken,
                successfulOauthHandler.Object,
                DbContext,
                DummyGuildId,
                user,
                dummyGuildMember
            );

            mockStep.Verify
            (
                service => service.FillDataAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}