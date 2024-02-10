//
//   IAuthTask.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.CtuAuth.Auth.Tasks
{
    /// <summary>
    /// Task used after auth steps to set Discord data.
    /// </summary>
    public interface IAuthTask
    {
        /// <summary>
        /// Executes the task job.
        /// </summary>
        /// <param name="data">The data of the authentication.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default);
    }
}