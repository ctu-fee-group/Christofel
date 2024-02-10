//
//   IPreAuthCondition.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.CtuAuth.Auth.Conditions
{
    /// <summary>
    /// Auth condition executed prior to the process of executing auth steps.
    /// </summary>
    public interface IPreAuthCondition
    {
        /// <summary>
        /// Checks the condition for the process, if failed, process is aborted.
        /// </summary>
        /// <param name="authData">The data of the authentication.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = default);
    }
}