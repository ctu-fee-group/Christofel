//
//   CtuAuthProcessConditionTests.NoDuplicate.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Christofel.CtuAuth.Auth.Conditions;
using Christofel.CtuAuth.Auth.Steps;
using Christofel.CtuAuth.Errors;
using Christofel.CtuAuth.Resolvers;
using Christofel.CtuAuth.Tests.Data.Ctu.Auth;
using Microsoft.Extensions.DependencyInjection;
using Remora.Results;
using Xunit;

namespace Christofel.CtuAuth.Tests.Ctu.Auth
{
    /// <summary>
    /// Tests condition <see cref="AllowsNoDuplicate"/>.
    /// </summary>
#pragma warning disable SA1649
    public class CtuAuthProcessConditionNoDuplicateTests : CtuAuthProcessConditionTests<NoDuplicateCondition>
#pragma warning restore SA1649
    {
        private class AuthDataTakerStep : IAuthStep
        {
            public IAuthData? Data { get; private set; }

            public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
            {
                Data = data;
                return Task.FromResult(Result.FromSuccess());
            }
        }

        private class AuthDataTakerCondition : IPreAuthCondition
        {
            public IAuthData? Data { get; private set; }

            public ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = default)
            {
                Data = authData;
                return ValueTask.FromResult(Result.FromSuccess());
            }
        }

        private readonly AuthDataTakerStep _authStep = new AuthDataTakerStep();
        private readonly AuthDataTakerCondition _authCondition = new AuthDataTakerCondition();

        /// <summary>
        /// Tests that the condition allows ctu side duplicates, even without allows.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
        [Fact]
        public async Task AllowsAuthenticatedCtuDuplicate()
        {
            var services = SetupConditionServices();

            var user = await DbContext
                .SetupUserToAuthenticateAsync(DummyUsername, 12454);

            var duplicateUser = (await DbContext.SetupAuthenticatedUserAsync(DummyUsername, 65324))
                .ToLinkUser();
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
            _authStep.Data.Should().NotBeNull();
            _authStep.Data.UnapprovedLinkedAccounts
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeEquivalentTo(
                    duplicateUser,
                    options => options
                        .Including(x => x.CtuUsername)
                        .Including(x => x.DiscordId));
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

            var duplicateUser = await DbContext.SetupAuthenticatedUserAsync("non colliding username", 12454);
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
            Assert.IsType<DuplicateError>(result.Error);
            _authStep.Data.Should().BeNull();
            _authCondition.Data.Should().NotBeNull();
            _authCondition.Data.UnapprovedLinkedAccounts
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeEquivalentTo(
                    duplicateUser,
                    options => options
                        .Including(x => x.CtuUsername)
                        .Including(x => x.DiscordId));
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
            _authStep.Data.Should().NotBeNull();
            _authStep.Data!.UnapprovedLinkedAccounts.Should().BeEmpty();
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
            _authStep.Data.Should().NotBeNull();
            _authStep.Data!.UnapprovedLinkedAccounts.Should().BeEmpty();
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
            _authStep.Data.Should().NotBeNull();
            _authStep.Data!.UnapprovedLinkedAccounts.Should().BeEmpty();
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
            _authStep.Data.Should().NotBeNull();
            _authStep.Data!.UnapprovedLinkedAccounts.Should().BeEmpty();
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
            _authStep.Data.Should().NotBeNull();
            _authStep.Data!.UnapprovedLinkedAccounts.Should().BeEmpty();
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
            await DbContext.SetupAuthenticatedUserAsync("set username 2", 12455);
            await DbContext.SetupAuthenticatedUserAsync("set username 2", 12455);
            await DbContext.SetupAuthenticatedUserAsync("set username 3", 12456);
            await DbContext.SetupAuthenticatedUserAsync("set username 5", 12456);
            await DbContext.SetupAuthenticatedUserAsync("set username 2", 12459);
            await DbContext.SetupAuthenticatedUserAsync("set username 2", 12458);

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
            _authStep.Data.Should().NotBeNull();
            _authStep.Data!.UnapprovedLinkedAccounts.Should().BeEmpty();
        }

        /// <inheritdoc />
        protected override IServiceProvider SetupConditionServices(Action<IServiceCollection>? configure = default)
        {
            return base.SetupConditionServices(services =>
            {
                services.AddScoped<DuplicateResolver>();
                services.AddTransient<IPreAuthCondition>(_ => _authCondition);
                services.AddTransient<IAuthStep>(_ => _authStep);

                configure?.Invoke(services);
            });
        }
    }
}
