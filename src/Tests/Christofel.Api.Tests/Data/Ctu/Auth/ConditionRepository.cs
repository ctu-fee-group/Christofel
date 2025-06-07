//
//   ConditionRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Conditions;
using Remora.Results;

namespace Christofel.Api.Tests.Data.Ctu.Auth
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
            /// <inheritdoc />
            public ValueTask<Result> CheckPreAsync
                (IAuthData authData, CancellationToken ct = default) => ValueTask.FromResult<Result>
                (new InvalidOperationError());
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