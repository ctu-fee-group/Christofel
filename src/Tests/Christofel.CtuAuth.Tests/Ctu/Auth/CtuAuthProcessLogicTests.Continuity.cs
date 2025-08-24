//
//   CtuAuthProcessLogicTests.Continuity.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CtuAuth.Auth.Steps;
using Christofel.CtuAuth.Auth.Tasks;
using Christofel.CtuAuth.Extensions;
using Christofel.CtuAuth.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Remora.Results;

namespace Christofel.CtuAuth.Tests.Ctu.Auth
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
        public async Task FailedConditionDoesntStartStepsNorTasks()
        {
            var stepMock = new Mock<StepRepository.MockStep>();
            var taskMock = new Mock<TaskRepository.MockTask>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.FailingCondition>()
                .AddTransient<IAuthStep>(_ => stepMock.Object)
                .AddTransient<IAuthTask>(_ => taskMock.Object)
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

            taskMock.Verify
            (
                service => service.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
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

        /// <summary>
        /// Tests that if task fails, other tasks are still executed, even if exception is thrown.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous task.</returns>
        [Fact]
        public async Task ExecutesOtherTasksOnFailedTasks()
        {
            var taskMock1 = new Mock<TaskRepository.MockTask>();
            var taskMock2 = new Mock<TaskRepository.MockTask>();
            var taskMock3 = new Mock<TaskRepository.MockTask>();
            var taskMock4 = new Mock<TaskRepository.MockTask>();
            var taskMock5 = new Mock<TaskRepository.MockTask>();

            Mock<TaskRepository.MockTask>[] mocks =
                [taskMock1, taskMock2, taskMock3, taskMock4, taskMock5];

            taskMock1.Setup(h => h.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>();
            taskMock2.Setup(h => h.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Result.FromError(new DummyError("dummy2"))));
            taskMock3.Setup(h => h.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Result.FromError(new DummyError("dummy3"))));
            taskMock4.Setup(h => h.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Result.FromError(new DummyError("dummy4"))));
            taskMock5.Setup(h => h.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>();

            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddCtuSuccessfulCST()
                .AddTransient<IAuthTask>(p => taskMock1.Object)
                .AddTransient<IAuthTask>(p => taskMock2.Object)
                .AddTransient<IAuthTask>(p => taskMock3.Object)
                .AddTransient<IAuthTask>(p => taskMock4.Object)
                .AddTransient<IAuthTask>(p => taskMock5.Object)
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

            foreach (var mock in mocks)
            {
                mock.Verify
                (
                    service => service.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                    Times.Once
                );
            }
        }
    }
}
