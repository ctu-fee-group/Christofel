//
//   ConditionRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CtuAuth.Auth.Conditions;
using Remora.Results;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth
{
    /// <summary>
    /// Class containing common <see cref="IPreAuthCondition"/>s.
    /// </summary>
    public class ConditionRepository
    {
        /// <summary>
        /// Condition that will always fail.
        /// </summary>
        public class FailingCondition : IPreAuthCondition
        {
            private readonly ResultError _error;

            /// <summary>
            /// Initializes a new instance of the <see cref="FailingCondition"/> class.
            /// </summary>
            /// <param name="error">The error to return.</param>
            public FailingCondition(ResultError? error = null)
            {
                _error = error ?? new InvalidOperationError();
            }

            /// <inheritdoc />
            public ValueTask<Result> CheckPreAsync
                (IAuthData authData, CancellationToken ct = default)
                => ValueTask.FromResult<Result>(_error);
        }

        /// <summary>
        /// Condition that will be always successful.
        /// </summary>
        public class SuccessfulCondition : IPreAuthCondition
        {
            /// <inheritdoc />
            public ValueTask<Result> CheckPreAsync
                (IAuthData authData, CancellationToken ct = default) => ValueTask.FromResult
                (Result.FromSuccess());
        }

        /// <summary>
        /// Condition that will always throw an exception.
        /// </summary>
        public class ExceptionThrowingCondition : IPreAuthCondition
        {
            /// <inheritdoc />
            public ValueTask<Result> CheckPreAsync
                (IAuthData authData, CancellationToken ct = default)
                => throw new InvalidOperationException();
        }

        /// <summary>
        /// Condition that may be used as a mock condition.
        /// </summary>
        public abstract class MockCondition : IPreAuthCondition
        {
            /// <inheritdoc />
            public abstract ValueTask<Result> CheckPreAsync
                (IAuthData authData, CancellationToken ct = default);
        }
    }
}
