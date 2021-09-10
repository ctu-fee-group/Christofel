//
//   TaskRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Tasks;
using Remora.Results;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    /// <summary>
    /// Repository containing <see cref="IAuthTask"/>s.
    /// </summary>
    public class TaskRepository
    {
        /// <summary>
        /// Task that will always fail.
        /// </summary>
        public class FailingTask : IAuthTask
        {
            /// <inheritdoc />
            public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default) => Task.FromResult<Result>
                (new InvalidOperationError());
        }

        /// <summary>
        /// Task that will always be successful.
        /// </summary>
        public class SuccessfulTask : IAuthTask
        {
            /// <inheritdoc />
            public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default) => Task.FromResult
                (Result.FromSuccess());
        }

        /// <summary>
        /// Task that will always throw an exception.
        /// </summary>
        public class ExceptionThrowingTask : IAuthTask
        {
            /// <inheritdoc />
            public Task<Result> ExecuteAsync
                (IAuthData data, CancellationToken ct = default) => throw new InvalidOperationException();
        }

        /// <summary>
        /// Task that can be used for mocking.
        /// </summary>
        public abstract class MockTask : IAuthTask
        {
            /// <inheritdoc />
            public abstract Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default);
        }
    }
}