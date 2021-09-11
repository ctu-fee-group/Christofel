//
//   NoDuplicateCondition.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Resolvers;
using Christofel.Api.GraphQL.Common;
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
            Duplicate duplicate = await _duplicates.ResolveDuplicateAsync(authData.LoadedUser, ct);
            var duplicityType = duplicate.Type;

            switch (duplicityType)
            {
                case DuplicityType.CtuSide:
                case DuplicityType.DiscordSide:
                    if (!authData.DbUser.DuplicityApproved)
                    {
                        _logger.LogWarning
                            ("Found a {Type} duplicate of <@{DiscordUser}>", duplicityType, duplicate.User?.DiscordId);
                        authData.DbUser.DuplicitUserId = duplicate.User?.UserId; // TODO: out better way to set the id.
                        return UserErrors.RejectedDuplicateUser;
                    }

                    break;
            }

            return Result.FromSuccess();
        }
    }
}