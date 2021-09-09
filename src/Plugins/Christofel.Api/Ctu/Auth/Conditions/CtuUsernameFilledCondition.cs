//
//   CtuUsernameFilledCondition.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Conditions
{
    public class CtuUsernameFilledCondition : IPreAuthCondition
    {
        public ValueTask<Result> CheckPreAsync
            (IAuthData authData, CancellationToken ct = new CancellationToken()) => ValueTask.FromResult
        (
            string.IsNullOrEmpty(authData.LoadedUser.CtuUsername)
                ? new InvalidOperationError("Cannot proceed if ctu username is null")
                : Result.FromSuccess()
        );
    }
}