//
//   IAuthTask.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Tasks
{
    public interface IAuthTask
    {
        public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default);
    }
}