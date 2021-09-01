using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Steps;
using Remora.Results;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    public class StepRepository
    {
        public class FailingStep : IAuthStep
        {
            public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
            {
                return Task.FromResult<Result>(new InvalidOperationError());
            }
        }
        
        public class SuccessfulStep : IAuthStep
        {
            public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
            {
                return Task.FromResult(Result.FromSuccess());
            }
        }
        
        public class ExceptionThrowingStep : IAuthStep
        {
            public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
            {
                throw new InvalidOperationException();
            }
        }
    }
}