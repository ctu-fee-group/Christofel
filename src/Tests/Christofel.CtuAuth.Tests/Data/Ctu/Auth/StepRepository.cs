//
//   StepRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CtuAuth;
using Christofel.CtuAuth.Auth.Steps;
using Remora.Results;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth
{
    /// <summary>
    /// Class containing common <see cref="IAuthStep"/>s.
    /// </summary>
    public class StepRepository
    {
        /// <summary>
        /// Step that will always fail.
        /// </summary>
        public class FailingStep : IAuthStep
        {
            private readonly ResultError _error;

            /// <summary>
            /// Initializes a new instance of the <see cref="FailingStep"/> class.
            /// </summary>
            /// <param name="error">The error to return.</param>
            public FailingStep(ResultError? error = null)
            {
                _error = error ?? new InvalidOperationError();
            }

            /// <inheritdoc />
            public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default) => Task.FromResult<Result>
                (_error);
        }

        /// <summary>
        /// Step that will always be successful.
        /// </summary>
        public class SuccessfulStep : IAuthStep
        {
            /// <inheritdoc />
            public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default) => Task.FromResult
                (Result.FromSuccess());
        }

        /// <summary>
        /// Step that will always throw an exception.
        /// </summary>
        public class ExceptionThrowingStep : IAuthStep
        {
            /// <inheritdoc />
            public Task<Result> FillDataAsync
                (IAuthData data, CancellationToken ct = default) => throw new InvalidOperationException();
        }

        /// <summary>
        /// Step that will set AuthenticatedAt field of the user.
        /// </summary>
        public class SetAuthenticatedAtStep : IAuthStep
        {
            /// <inheritdoc />
            public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
            {
                data.DbUser.AuthenticatedAt = DateTime.Now;
                return Task.FromResult(Result.FromSuccess());
            }
        }

        /// <summary>
        /// Step that should be used for mocking.
        /// </summary>
        public abstract class MockStep : IAuthStep
        {
            /// <inheritdoc />
            public abstract Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default);
        }
    }
}
