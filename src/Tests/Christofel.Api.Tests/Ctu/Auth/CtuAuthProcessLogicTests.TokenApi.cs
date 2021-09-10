//
//   CtuAuthProcessLogicTests.TokenApi.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Extensions;
using Christofel.Api.OAuth;
using Christofel.Api.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Christofel.Api.Tests.Ctu.Auth
{
    /// <summary>
    /// Tests that ctu auth process correctly handles response of <see cref="ICtuTokenApi"/>.
    /// </summary>
#pragma warning disable SA1649
    public class CtuAuthProcessLogicTokenApiTests : CtuAuthProcessLogicTests
#pragma warning restore SA1649
    {
        /// <summary>
        /// Tests that if there was an error retrieving the username, error will be returned.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task FailedUsernameRetrievalReturnsError()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
                .AddLogging(b => b.ClearProviders())
                .BuildServiceProvider();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = CreateDummyGuildMember(user);

            var process = services.GetRequiredService<CtuAuthProcess>();

            var failingOauthHandler = new Mock<ICtuTokenApi>();
            failingOauthHandler
                .Setup(tokenApi => tokenApi.CheckTokenAsync(DummyAccessToken, It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>();

            var result =
                await process.FinishAuthAsync
                (
                    DummyAccessToken,
                    failingOauthHandler.Object,
                    DbContext,
                    DummyGuildId,
                    user,
                    dummyGuildMember
                );

            Assert.False(result.IsSuccess);
        }

        /// <summary>
        /// Tests that successful retrieval of username will return success.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task SuccessfulUsernameRetrievalReturnsSuccess()
        {
            IServiceProvider services = new ServiceCollection()
                .AddCtuAuthProcess()
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
    }
}