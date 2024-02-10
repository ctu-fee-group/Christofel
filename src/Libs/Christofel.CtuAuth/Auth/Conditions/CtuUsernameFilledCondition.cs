//
//   CtuUsernameFilledCondition.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.CtuAuth.Auth.Conditions
{
    /// <summary>
    /// Condition that checks whether the loaded user has filled ctu username.
    /// </summary>
    public class CtuUsernameFilledCondition : IPreAuthCondition
    {
        /// <inheritdoc />
        public ValueTask<Result> CheckPreAsync
            (IAuthData authData, CancellationToken ct = default) => ValueTask.FromResult
        (
            string.IsNullOrEmpty(authData.LoadedUser.CtuUsername)
                ? new InvalidOperationError("Cannot proceed if ctu username is null")
                : Result.FromSuccess()
        );
    }
}