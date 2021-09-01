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
            public ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = new CancellationToken())
            {
                return ValueTask.FromResult<Result>(new InvalidOperationError());
            }
        }
        
        public class SuccessfulCondition : IPreAuthCondition
        {
            public ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = new CancellationToken())
            {
                return ValueTask.FromResult<Result>(Result.FromSuccess());
            }
        }
        
        public class ExceptionThrowingCondition : IPreAuthCondition
        {
            public ValueTask<Result> CheckPreAsync(IAuthData authData, CancellationToken ct = new CancellationToken())
            {
                throw new InvalidOperationException();
            }
        }
    }
}