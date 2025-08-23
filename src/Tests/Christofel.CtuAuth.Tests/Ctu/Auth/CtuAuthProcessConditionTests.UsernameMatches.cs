//
//   CtuAuthProcessConditionTests.UsernameMatches.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Christofel.CtuAuth;
using Christofel.CtuAuth.Auth.Conditions;
using Christofel.CtuAuth.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Christofel.CtuAuth.Tests.Ctu.Auth
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
        /// <param name="dbUsername">The username saved in database.</param>
        /// <param name="authUsername">The username used for authentication.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Theory]
        [InlineData("db", "auth")]
        [InlineData("db", "db1")]
        [InlineData("db", "1db")]
        [InlineData("1db", "db")]
        [InlineData("db1", "db")]
        public async Task DoesNotAllowNonMatchingFilledUsername(string dbUsername, string authUsername)
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync(dbUsername);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, authUsername);

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
        /// <param name="username">The username to use for auth.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Theory]
        [InlineData("username")]
        [InlineData("user")]
        [InlineData("user123")]
        [InlineData("123user")]
        [InlineData("_")]
        public async Task AllowsMatchingUsername(string username)
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync(username);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, username);

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

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, string.Empty);

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
