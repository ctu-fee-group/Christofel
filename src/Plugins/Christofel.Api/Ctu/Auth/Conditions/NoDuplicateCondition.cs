//
//   NoDuplicateCondition.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Resolvers;
using Christofel.Api.GraphQL.Common;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Conditions
{
    /// <summary>
    /// Condition that checks that the user is not a duplicate, or is an approved duplicate.
    /// </summary>
    public class NoDuplicateCondition : IPreAuthCondition
    {
        private readonly DuplicateResolver _duplicates;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoDuplicateCondition"/> class.
        /// </summary>
        /// <param name="duplicates">Resolver of the duplicates.</param>
        public NoDuplicateCondition(DuplicateResolver duplicates)
        {
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
                        return UserErrors.RejectedDuplicateUser;
                    }

                    break;
            }

            return Result.FromSuccess();
        }
    }
}