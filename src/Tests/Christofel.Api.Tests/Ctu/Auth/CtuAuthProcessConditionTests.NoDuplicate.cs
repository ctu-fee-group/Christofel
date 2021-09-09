//
//   CtuAuthProcessConditionTests.NoDuplicate.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Conditions;
using Christofel.Api.Ctu.Resolvers;
using Christofel.Api.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Christofel.Api.Tests.Ctu.Auth
{
    public class CtuAuthProcessConditionNoDuplicateTests : CtuAuthProcessConditionTests<NoDuplicateCondition>
    {
        [Fact]
        public async Task DoesNotAllowAuthenticatedCtuDuplicate()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync(_dummyUsername, 12454);

            await _dbContext.SetupAuthenticatedUserAsync(_dummyUsername, 65324);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

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
        public async Task DoesNotAllowAuthenticatedDiscordDuplicate()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync(_dummyUsername, 12454);

            await _dbContext.SetupAuthenticatedUserAsync("non colliding username", 12454);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

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
        public async Task AllowsMatchingDuplicate()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync(_dummyUsername, 12454);

            await _dbContext.SetupAuthenticatedUserAsync(_dummyUsername, 12454);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

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

        [Fact]
        public async Task AllowsNonAuthenticatedCtuDuplicate()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync(_dummyUsername, 12454);

            await _dbContext.SetupUserToAuthenticateAsync(_dummyUsername, 6234);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

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

        [Fact]
        public async Task AllowsNonAuthenticatedDiscordDuplicate()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync("username", 12454);

            await _dbContext.SetupUserToAuthenticateAsync("non colliding username", 12454);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, "username");

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync
            (
                _dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember
            );

            Assert.True(result.IsSuccess);
        }


        [Fact]
        public async Task AllowsApprovedDiscordDuplicate()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync("username", 12454);
            user.DuplicityApproved = true; // approve duplicate

            await _dbContext.SetupAuthenticatedUserAsync("non colliding username", 12454);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, "username");

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync
            (
                _dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember
            );

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AllowsApprovedCtuDuplicate()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync(_dummyUsername, 12454);
            user.DuplicityApproved = true; // approve duplicate

            await _dbContext.SetupAuthenticatedUserAsync(_dummyUsername, 67345);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

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

        [Fact]
        public async Task AllowsNoDuplicate()
        {
            var services = SetupConditionServices();

            var user = await _dbContext
                .SetupUserToAuthenticateAsync("set username", 12454);
            await _dbContext.SetupAuthenticatedUserAsync("non colliding username", 67345);

            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, "set username");

            var process = services.GetRequiredService<CtuAuthProcess>();
            var result = await process.FinishAuthAsync
            (
                _dummyAccessToken, successfulOauthHandler.Object, _dbContext,
                _dummyGuildId,
                user, dummyGuildMember
            );

            Assert.True(result.IsSuccess);
        }

        protected override IServiceProvider SetupConditionServices(Action<IServiceCollection>? configure = default)
        {
            return base.SetupConditionServices(services => services.AddScoped<DuplicateResolver>());
        }
    }
}