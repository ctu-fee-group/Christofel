//
//   StepRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Steps;
using Remora.Results;

namespace Christofel.Api.Tests.Data.Ctu.Auth
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
            /// <inheritdoc />
            public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default) => Task.FromResult<Result>
                (new InvalidOperationError());
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