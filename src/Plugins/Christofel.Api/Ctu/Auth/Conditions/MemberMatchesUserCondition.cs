//
//   MemberMatchesUserCondition.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Conditions
{
    public class MemberMatchesUserCondition : IPreAuthCondition
    {
        public ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = default)
        {
            if (!authData.GuildUser.User.HasValue)
            {
                return ValueTask.FromResult<Result>
                (
                    new InvalidOperationError
                        ("Cannot proceed as guild member user is not set, cannot check for match with database")
                );
            }

            var user = authData.GuildUser.User.Value;
            return ValueTask.FromResult
            (
                user.ID == authData.DbUser.DiscordId
                    ? Result.FromSuccess()
                    : new InvalidOperationError("Cannot proceed with guild member ID not matching db user discord ID")
            );
        }
    }
}