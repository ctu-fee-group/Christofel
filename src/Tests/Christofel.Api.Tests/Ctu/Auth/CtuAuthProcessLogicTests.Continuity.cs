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
    public class CtuAuthProcessLogicContinuityTests : CtuAuthProcessLogicTests
    {
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

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result =
                await process.FinishAuthAsync
                (
                    _dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                    _dummyGuildId,
                    user, dummyGuildMember
                );

            taskMock.Verify
            (
                service => service.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

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

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result =
                await process.FinishAuthAsync
                (
                    _dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                    _dummyGuildId,
                    user, dummyGuildMember
                );

            stepMock.Verify
            (
                service => service.FillDataAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }


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

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);
            var successfulOauthHandler = GetMockedTokenApi(user);

            var process = services.GetRequiredService<CtuAuthProcess>();
            await process.FinishAuthAsync
            (
                _dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember
            );

            taskMock.Verify
            (
                service => service.ExecuteAsync(It.IsAny<IAuthData>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
}