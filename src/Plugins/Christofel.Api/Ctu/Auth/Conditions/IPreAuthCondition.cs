//
//   IPreAuthCondition.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Conditions
{
    /// <summary>
    /// Auth condition executed prior to the process of executing auth steps
    /// </summary>
    public interface IPreAuthCondition
    {
        /// <summary>
        /// Checks the condition for the process, if failed, process is aborted
        /// </summary>
        /// <param name="authData"></param>
        /// <param name="ct"></param>
        /// <returns>Successful result if passed, error on failure</returns>
        public ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = default);
    }
}