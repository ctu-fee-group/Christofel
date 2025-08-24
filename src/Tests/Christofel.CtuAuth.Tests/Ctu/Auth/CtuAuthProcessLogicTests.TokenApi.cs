//
//   CtuAuthProcessLogicTests.TokenApi.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Christofel.CtuAuth.Extensions;
using Christofel.CtuAuth.Tests.Data.Ctu.Auth;
using Christofel.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Remora.Results;
using Xunit;

namespace Christofel.CtuAuth.Tests.Ctu.Auth
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
                .Setup(tokenApi => tokenApi.GetUser(DummyAccessToken))
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
            Assert.IsType<ExceptionError>(result.Error);
            Assert.IsType<InvalidOperationException>(((ExceptionError)result.Error).Exception);
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
