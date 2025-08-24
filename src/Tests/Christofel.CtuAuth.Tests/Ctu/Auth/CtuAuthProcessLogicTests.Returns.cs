//
//   CtuAuthProcessLogicTests.Returns.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Christofel.CtuAuth;
using Christofel.CtuAuth.Auth;
using Christofel.CtuAuth.Auth.Conditions;
using Christofel.CtuAuth.Auth.Steps;
using Christofel.CtuAuth.Auth.Tasks;
using Christofel.CtuAuth.Extensions;
using Christofel.CtuAuth.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Results;
using Xunit;
using static Christofel.CtuAuth.Tests.Data.Ctu.Auth.TaskRepository;

namespace Christofel.CtuAuth.Tests.Ctu.Auth
{
    /// <summary>
    /// Tests that the ctu auth process returns correct results.
    /// </summary>
#pragma warning disable SA1649
    public class CtuAuthProcessLogicReturnsTests : CtuAuthProcessLogicTests
#pragma warning restore SA1649
    {
        /// <summary>
        /// Tests that failed condition returns an error.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task FailedConditionReturnsError()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddTransient<IPreAuthCondition, ConditionRepository.FailingCondition>(_ => new ConditionRepository.FailingCondition(new DummyError("dummy")))
                .AddAuthCondition<ConditionRepository.FailingCondition>()
                .AddAuthStep<StepRepository.FailingStep>()
                .AddAuthTask<TaskRepository.FailingTask>()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result =
                await process.FinishAuthAsync
                (
                    DummyAccessToken,
                    successfulOauthHandler.Object,
                    DbContext,
                    DummyGuildId,
                    user,
                    dummyGuildMember
                );

            Assert.False(result.IsSuccess);
            Assert.IsType<DummyError>(result.Error);
        }

        /// <summary>
        /// Tests that successful condition will return success.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task SuccessfulConditionReturnsSuccess()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.SuccessfulCondition>()
                .AddAuthStep<StepRepository.SuccessfulStep>()
                .AddAuthTask<TaskRepository.SuccessfulTask>()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result =
                await process.FinishAuthAsync
                (
                    DummyAccessToken,
                    successfulOauthHandler.Object,
                    DbContext,
                    DummyGuildId,
                    user,
                    dummyGuildMember
                );

            Assert.True(result.IsSuccess);
        }

        /// <summary>
        /// Tests that failed step will return error.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task FailedStepReturnsError()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.SuccessfulCondition>()
                .AddTransient<IAuthStep>(_ => new StepRepository.FailingStep(new DummyError("dummy")))
                .AddAuthStep<StepRepository.FailingStep>()
                .AddAuthTask<TaskRepository.FailingTask>()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result =
                await process.FinishAuthAsync
                (
                    DummyAccessToken,
                    successfulOauthHandler.Object,
                    DbContext,
                    DummyGuildId,
                    user,
                    dummyGuildMember
                );

            Assert.False(result.IsSuccess);
            Assert.IsType<DummyError>(result.Error);
        }

        /// <summary>
        /// Tests that failed task will return an error.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task FailedTaskReturnsError()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddTransient<IAuthTask, TaskRepository.FailingTask>(_ => new FailingTask(new DummyError("dummy")))
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result =
                await process.FinishAuthAsync
                (
                    DummyAccessToken,
                    successfulOauthHandler.Object,
                    DbContext,
                    DummyGuildId,
                    user,
                    dummyGuildMember
                );

            Assert.False(result.IsSuccess);
            Assert.IsType<SoftAuthError>(result.Error);
            Assert.NotNull(result.Inner);
            Assert.IsType<DummyError>(result.Inner.Error);
            Assert.Equal("dummy", ((DummyError)result.Inner.Error).Dummy);
        }

        /// <summary>
        /// Tests that failed task will return multiple errors from tasks.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task FailedTaskReturnsAggregatedErrors()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddTransient<IAuthTask, TaskRepository.FailingTask>(_ => new FailingTask(new DummyError("dummy")))
                .AddTransient<IAuthTask, TaskRepository.FailingTask>(_ => new FailingTask(new DummyError("dummy1")))
                .AddAuthTask<TaskRepository.SuccessfulTask>()
                .AddAuthStep<StepRepository.SuccessfulStep>()
                .AddAuthCondition<ConditionRepository.SuccessfulCondition>()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result =
                await process.FinishAuthAsync
                (
                    DummyAccessToken,
                    successfulOauthHandler.Object,
                    DbContext,
                    DummyGuildId,
                    user,
                    dummyGuildMember
                );

            Assert.False(result.IsSuccess);
            Assert.IsType<SoftAuthError>(result.Error);
            Assert.NotNull(result.Inner);
            Assert.IsType<AggregateError>(result.Inner.Error);
            Assert.Collection(
                ((AggregateError)result.Inner.Error).Errors,
                e =>
                {
                    Assert.IsType<DummyError>(e.Error);
                    Assert.Equal("dummy", ((DummyError)e.Error).Dummy);
                },
                e =>
                {
                    Assert.IsType<DummyError>(e.Error);
                    Assert.Equal("dummy1", ((DummyError)e.Error).Dummy);
                });
        }
    }
}
