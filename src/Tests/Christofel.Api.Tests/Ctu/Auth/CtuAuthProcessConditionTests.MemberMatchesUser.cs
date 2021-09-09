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
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Xunit;

namespace Christofel.Api.Tests.Ctu.Auth
{
    public class
        CtuAuthProcessConditionMemberMatchesUserTests : CtuAuthProcessConditionTests<MemberMatchesUserCondition>
    {
        [Fact]
        public async Task DoesNotAllowMissingUser()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = new GuildMember
            (
                default, default, new List<Snowflake>(), DateTimeOffset.Now, default,
                default, default
            );
            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, _dummyUsername);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync
            (
                _dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember
            );

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task DoesNotAllowNotMatchingMember()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = new GuildMember
            (
                new User(new Snowflake(111), _dummyUsername, 124, default),
                default,
                new List<Snowflake>(),
                DateTimeOffset.Now,
                default,
                default,
                default
            );

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, _dummyUsername);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync
            (
                _dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember
            );

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task AllowsMatchingMember()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = new GuildMember
            (
                new User(user.DiscordId, _dummyUsername, 124, default),
                default,
                new List<Snowflake>(),
                DateTimeOffset.Now,
                default,
                default,
                default
            );

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, _dummyUsername);

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync
            (
                _dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember
            );

            Assert.True(result.IsSuccess);
        }
    }
}