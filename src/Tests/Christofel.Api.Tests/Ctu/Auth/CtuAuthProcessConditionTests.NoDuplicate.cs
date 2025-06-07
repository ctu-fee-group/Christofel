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
    /// <summary>
    /// Tests condition <see cref="AllowsNoDuplicate"/>.
    /// </summary>
#pragma warning disable SA1649
    public class CtuAuthProcessConditionNoDuplicateTests : CtuAuthProcessConditionTests<NoDuplicateCondition>
#pragma warning restore SA1649
    {
        /// <summary>
        /// Tests that the condition does not allow non approved duplicate.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task DoesNotAllowAuthenticatedCtuDuplicate()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync(DummyUsername, 12454);

            await DbContext.SetupAuthenticatedUserAsync(DummyUsername, 65324);
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

            Assert.False(result.IsSuccess);
        }

        /// <summary>
        /// Tests that the condition does not allow non approved duplicate.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task DoesNotAllowAuthenticatedDiscordDuplicate()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync(DummyUsername, 12454);

            await DbContext.SetupAuthenticatedUserAsync("non colliding username", 12454);
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

            Assert.False(result.IsSuccess);
        }

        /// <summary>
        /// Tests that the condition allows matching duplicate. AKA duplicate of type Both.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task AllowsMatchingDuplicate()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync(DummyUsername, 12454);

            await DbContext.SetupAuthenticatedUserAsync(DummyUsername, 12454);
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
        /// Tests that the condition allows duplicate when the duplicate is not authenticated.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task AllowsNonAuthenticatedCtuDuplicate()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync(DummyUsername, 12454);

            await DbContext.SetupUserToAuthenticateAsync(DummyUsername, 6234);
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
        /// Tests that the condition allows duplicate when the duplicate is not authenticated.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task AllowsNonAuthenticatedDiscordDuplicate()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync("username", 12454);

            await DbContext.SetupUserToAuthenticateAsync("non colliding username", 12454);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, "username");

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
        /// Tests that the condition allows duplicate when it is approved.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task AllowsApprovedDiscordDuplicate()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync("username", 12454);
            user.DuplicityApproved = true; // approve duplicate

            await DbContext.SetupAuthenticatedUserAsync("non colliding username", 12454);
            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, "username");

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
        /// Tests that the condition allows duplicate when it is approved.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task AllowsApprovedCtuDuplicate()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync(DummyUsername, 12454);
            user.DuplicityApproved = true; // approve duplicate

            await DbContext.SetupAuthenticatedUserAsync(DummyUsername, 67345);
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
        /// Tests that the condition allows no matching duplicate.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task AllowsNoDuplicate()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync("set username", 12454);
            await DbContext.SetupAuthenticatedUserAsync("non colliding username", 67345);

            var dummyGuildMember = GuildMemberRepository.CreateDummyGuildMember(user);

            var successfulOauthHandler = OauthTokenApiRepository.GetMockedTokenApi(user, "set username");

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

        /// <inheritdoc />
        protected override IServiceProvider SetupConditionServices(Action<IServiceCollection>? configure = default)
        {
            return base.SetupConditionServices(services => services.AddScoped<DuplicateResolver>());
        }
    }
}