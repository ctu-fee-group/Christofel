//
//   CtuUsernameMatchesCondition.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Conditions
{
    public class CtuUsernameMatchesCondition : IPreAuthCondition
    {
        public ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = default)
        {
            if (authData.DbUser.CtuUsername is null)
            {
                return ValueTask.FromResult(Result.FromSuccess());
            }

            return ValueTask.FromResult
            (
                authData.LoadedUser.CtuUsername == authData.DbUser.CtuUsername
                    ? Result.FromSuccess()
                    : new InvalidOperationError("CtuUsername in the database does not match the loaded one")
            );
        }
    }
}