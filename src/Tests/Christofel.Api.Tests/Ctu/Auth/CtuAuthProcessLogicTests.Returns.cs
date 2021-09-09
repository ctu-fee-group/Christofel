//
//   CtuAuthProcessLogicTests.Returns.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Extensions;
using Christofel.Api.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Christofel.Api.Tests.Ctu.Auth
{
    public class CtuAuthProcessLogicReturnsTests : CtuAuthProcessLogicTests
    {
        [Fact]
        public async Task FailedConditionReturnsError()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.FailingCondition>()
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

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task SuccessfulConditionReturnsSuccess()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthCondition<ConditionRepository.SuccessfulCondition>()
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

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task FailedStepReturnsError()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthStep<StepRepository.FailingStep>()
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

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task FailedTaskReturnsError()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddAuthTask<TaskRepository.FailingTask>()
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

            Assert.False(result.IsSuccess);
        }
    }
}