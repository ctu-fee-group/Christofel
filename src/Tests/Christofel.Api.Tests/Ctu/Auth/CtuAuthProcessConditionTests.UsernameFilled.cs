//
//   CtuAuthProcessConditionTests.UsernameFilled.cs
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
    public class CtuAuthProcessConditionUsernameFilledTest : CtuAuthProcessConditionTests<CtuUsernameFilledCondition>
    {
        [Fact]
        public async Task DoesNotAllowNonFilledUsername()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, "");

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
        public async Task AllowsFilledUsername()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync();
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, "filled");

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