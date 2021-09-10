//
//   IAuthStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Steps
{
    /// <summary>
    /// Individual step of the auth process used to
    /// assign roles etc.
    /// </summary>
    public interface IAuthStep
    {
        /// <summary>
        /// Handle the step, call next if the process
        /// should continue.
        /// </summary>
        /// <param name="data">The data of the authentication.</param>
        /// <param name="ct">The cancellation token for the operation.</param>
        /// <returns>A result that may not have succeeded.</returns>
        public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default);
    }
}