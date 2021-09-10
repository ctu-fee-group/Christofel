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
    public class ConditionRepository
    {
        public class FailingCondition : IPreAuthCondition
        {
            public ValueTask<Result> CheckPreAsync
                (IAuthData authData, CancellationToken ct = default) => ValueTask.FromResult<Result>
                (new InvalidOperationError());
        }

        public class SuccessfulCondition : IPreAuthCondition
        {
            public ValueTask<Result> CheckPreAsync
                (IAuthData authData, CancellationToken ct = default) => ValueTask.FromResult
                (Result.FromSuccess());
        }

        public class ExceptionThrowingCondition : IPreAuthCondition
        {
            public ValueTask<Result> CheckPreAsync
                (IAuthData authData, CancellationToken ct = default)
                => throw new InvalidOperationException();
        }

        public abstract class MockCondition : IPreAuthCondition
        {
            public abstract ValueTask<Result> CheckPreAsync
                (IAuthData authData, CancellationToken ct = default);
        }
    }
}