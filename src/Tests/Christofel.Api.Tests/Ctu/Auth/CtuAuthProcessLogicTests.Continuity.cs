//
//   CtuAuthProcessLogicTests.Continuity.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Steps;
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
    /// Tests that the ctu auth process does not what souldn't be.
    /// </summary>
#pragma warning disable SA1649
    public class CtuAuthProcessLogicContinuityTests : CtuAuthProcessLogicTests
#pragma warning restore SA1649
    {
        /// <summary>
        /// Tests that if condition fails, tasks won't be started.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous task.</returns>
        [Fact]
        public async Task FailedConditionDoesntStartTasks()
        {
            var taskMock = new Mock<TaskRepository.MockTask>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.FailingCondition>()
                .AddScoped<IAuthTask, TaskRepository.MockTask>(p => taskMock.Object)
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

            taskMock.Verify
            (
                service => service.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        /// <summary>
        /// Tests that if condition fails, steps won't be started.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous task.</returns>
        [Fact]
        public async Task FailedConditionDoesntStartSteps()
        {
            var stepMock = new Mock<StepRepository.MockStep>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.FailingCondition>()
                .AddScoped<IAuthStep, StepRepository.MockStep>(p => stepMock.Object)
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

            stepMock.Verify
            (
                service => service.FillDataAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        /// <summary>
        /// Tests that if step fails, tasks won't be started.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous task.</returns>
        [Fact]
        public async Task FailedStepDoesntStartTasks()
        {
            var taskMock = new Mock<TaskRepository.MockTask>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthStep<StepRepository.FailingStep>()
                .AddScoped<IAuthTask, TaskRepository.MockTask>(p => taskMock.Object)
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

            taskMock.Verify
            (
                service => service.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
}