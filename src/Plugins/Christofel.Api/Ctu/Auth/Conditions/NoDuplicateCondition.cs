//
//   NoDuplicateCondition.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Resolvers;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Conditions
{
    /// <summary>
    /// Condition that checks that the user is not a duplicate, or is an approved duplicate.
    /// </summary>
    public class NoDuplicateCondition : IPreAuthCondition
    {
        private readonly DuplicateResolver _duplicates;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoDuplicateCondition"/> class.
        /// </summary>
        /// <param name="duplicates">The resolver of the duplicates.</param>
        /// <param name="logger">The logger.</param>
        public NoDuplicateCondition(DuplicateResolver duplicates, ILogger<NoDuplicateCondition> logger)
        {
            _logger = logger;
            _duplicates = duplicates;
        }

        /// <inheritdoc />
        public async ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = default)
        {
            var duplicates = await _duplicates.ResolveDuplicateAsync(authData.LoadedUser, ct);
            if (authData.DbUser.DuplicityApproved)
            {
                return Result.FromSuccess();
            }

            if (!duplicates.DuplicateFound)
            {
                return Result.FromSuccess();
            }

            if (duplicates.Discord is null && duplicates.Ctu is null)
            {
                return Result.FromSuccess();
            }

            authData.DbUser.DuplicitUserId
                ??= duplicates.Ctu?.Users.FirstOrDefault()?.UserId; // TODO: find out better way to set the id.
            authData.DbUser.DuplicitUserId
                ??= duplicates.Discord?.Users.FirstOrDefault()?.UserId; // TODO: find out better way to set the id.

            foreach (var duplicate in duplicates.Discord?.Users ?? Array.Empty<DuplicateUser>())
            {
                authData.AddLinkedAccount(duplicate);

                _logger.LogWarning
                (
                    "Found a {Type} duplicate of <@{DiscordUser}>, allowing, (partially) deauthenticating old account.",
                    DuplicityType.DiscordSide,
                    duplicate.DiscordId
                );
            }
            foreach (var duplicate in duplicates.Ctu?.Users ?? Array.Empty<DuplicateUser>())
            {
                authData.AddLinkedAccount(duplicate);

                _logger.LogWarning
                (
                    "Found a {Type} duplicate of <@{DiscordUser}>, allowing, (partially) deauthenticating old account.",
                    DuplicityType.CtuSide,
                    duplicate.DiscordId
                );
            }

            return Result.FromSuccess();
        }
    }
}