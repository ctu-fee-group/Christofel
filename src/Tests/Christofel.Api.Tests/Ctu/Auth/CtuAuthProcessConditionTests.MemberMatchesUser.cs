//
//   CtuAuthProcessConditionTests.MemberMatchesUser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.API;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Xunit;

namespace Christofel.Api.Tests.Ctu.Auth
{
    /// <summary>
    /// Tests for condition <see cref="MemberMatchesUserCondition"/>.
    /// </summary>
    public class
#pragma warning disable SA1649
        CtuAuthProcessConditionMemberMatchesUserTests : CtuAuthProcessConditionTests<MemberMatchesUserCondition>
#pragma warning restore SA1649
    {
        /// <summary>
        /// Tests that the condition does not allow <see cref="GuildMember"/> with missing user.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        [Fact]
        public async Task DoesNotAllowMissingUser()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = new GuildMember
            (
                default,
                default,
                default,
                new List<Snowflake>(),
                DateTimeOffset.Now,
                default,
                default,
                default
            );
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

            Assert.False(result.IsSuccess);
        }

        /// <summary>
        /// Tests that condition does not allow <see cref="GuildMember"/> with non matching id.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task DoesNotAllowNotMatchingMember()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = new GuildMember
            (
                new User(new Snowflake(111, Constants.DiscordEpoch), DummyUsername, 124, default),
                default,
                default,
                new List<Snowflake>(),
                DateTimeOffset.Now,
                default,
                default,
                default
            );

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

            Assert.False(result.IsSuccess);
        }

        /// <summary>
        /// Tests that condition does allows <see cref="GuildMember"/> with matching id.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task AllowsMatchingMember()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = new GuildMember
            (
                new User(user.DiscordId, DummyUsername, 124, default),
                default,
                default,
                new List<Snowflake>(),
                DateTimeOffset.Now,
                default,
                default,
                default
            );

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