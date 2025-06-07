//
//   CtuAuthProcessConditionTests.UsernameMatches.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Christofel.Api.Tests.Ctu.Auth
{
    /// <summary>
    /// Tests condition <see cref="CtuUsernameMatchesCondition"/>.
    /// </summary>
#pragma warning disable SA1649
    public class CtuAuthProcessConditionUsernameMatchesTests : CtuAuthProcessConditionTests<CtuUsernameMatchesCondition>
#pragma warning restore SA1649
    {
        /// <summary>
        /// Tests that the condition does not allow non matching filled username.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task DoesNotAllowNonMatchingFilledUsername()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync("non matching username");
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, "real username");

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync
            (
                DummyAccessToken,
                successfulOauthHandler.Object,
                DbContext,
                DummyGuildId,
                user,
                dummyGuildMember
            );

            Assert.False(result.IsSuccess);
        }

        /// <summary>
        /// Tests that the condition allows matching username.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task AllowsMatchingUsername()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync(DummyUsername);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, DummyUsername);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync
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
        /// Tests that the condition allows non filled username.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task AllowsNonFilledUsername()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, DummyUsername);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync
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