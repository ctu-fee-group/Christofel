//
//   CtuAuthProcessLogicTests.Data.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    /// <summary>
    /// Tests that the ctu auth process sets correct data and saves to the database.
    /// </summary>
#pragma warning disable SA1649
    public class CtuAuthProcessLogicDataTests : CtuAuthProcessLogicTests
#pragma warning restore SA1649
    {
        /// <summary>
        /// Tests that ctu username won't be changed if it was set already.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task CtuUsernameIsNotSetIfFilled()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.FailingCondition>()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            string alreadySetUsername = "already set username";
            user.CtuUsername = alreadySetUsername;
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

            Assert.Matches(alreadySetUsername, user.CtuUsername);
        }

        /// <summary>
        /// Tests that if one task failed, other will be executed.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
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
                .AddAuthTask<TaskRepository.FailingTask>()
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
        /// Tests that ctu username will be set after failing conditions and saved to the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task CtuUsernameIsSetAfterFailedConditions()
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.FailingCondition>()
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

            Assert.NotNull(user.CtuUsername);
            Assert.Matches(DummyUsername, user.CtuUsername);
        }

        /// <summary>
        /// Tests that context will be saved after failed conditions.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task ContextIsSavedAfterFailedConditions()
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.FailingCondition>()
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

            DbContext.ChangeTracker.Clear();

            var savedUser = DbContext.Users.First();
            Assert.NotNull(savedUser.CtuUsername);
            Assert.Matches(DummyUsername, savedUser.CtuUsername);
        }

        /// <summary>
        /// Tests that ctu username will be set after successful conditions and saved to the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task CtuUsernameIsSetAfterSuccessfulConditions()
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.SuccessfulCondition>()
                .AddAuthStep<StepRepository.FailingStep>()
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

            Assert.NotNull(user.CtuUsername);
            Assert.Matches(DummyUsername, user.CtuUsername);
        }

        /// <summary>
        /// Tests that context will be saved after successful conditions.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task ContextIsSavedAfterSuccessfulConditions()
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.SuccessfulCondition>()
                .AddAuthStep<StepRepository.FailingStep>()
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

            DbContext.ChangeTracker.Clear();

            var savedUser = DbContext.Users.First();
            Assert.NotNull(savedUser.CtuUsername);
            Assert.Matches(DummyUsername, savedUser.CtuUsername);
        }

        /// <summary>
        /// Tests that context will be saved after successful steps.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task ContextIsSavedAfterSuccessfulSteps()
        {
            var services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthStep<StepRepository.SetAuthenticatedAtStep>()
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

            DbContext.ChangeTracker.Clear();

            var savedUser = DbContext.Users.First();
            Assert.NotNull(savedUser.AuthenticatedAt);
        }
    }
}