using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu;
using Christofel.Api.Ctu.Auth.Tasks;
using Remora.Results;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    public class TaskRepository
    {
        public class FailingTask : IAuthTask
        {
            public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
            {
                return Task.FromResult<Result>(new InvalidOperationError());
            }
        }
        
        public class SuccessfulTask : IAuthTask
        {
            public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
            {
                return Task.FromResult(Result.FromSuccess());
            }
        }
        
        public class ExceptionThrowingTask : IAuthTask
        {
            public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
            {
                throw new InvalidOperationException();
            }
        }

        public abstract class MockTask : IAuthTask
        {
            public abstract Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default);
        }
    }
}