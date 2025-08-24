//
//   TaskRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CtuAuth.Auth.Tasks;
using Remora.Results;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth
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
            private readonly ResultError _error;

            /// <summary>
            /// Initializes a new instance of the <see cref="FailingTask"/> class.
            /// </summary>
            /// <param name="error">The error to return.</param>
            public FailingTask(ResultError? error = null)
            {
                _error = error ?? new InvalidOperationError();
            }

            /// <inheritdoc />
            public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
                => Task.FromResult<Result>(_error);
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
