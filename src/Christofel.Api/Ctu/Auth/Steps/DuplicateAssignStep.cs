using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Steps
{
    public class DuplicateAssignStep : IAuthStep
    {
        public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            if (!data.StepData.TryGetValue("Duplicate", out var duplicateObj))
            {
                return Task.FromResult<Result>(new InvalidOperationError(
                    "Could not find duplicate in step data. Did you forget to register duplicate condition?"));
            }

            var duplicate = (Duplicate)duplicateObj;
            if (duplicate.Type == DuplicityType.Both)
            {
                duplicate.User.AuthenticatedAt = DateTime.Now;
                data.DbContext.Remove(data.DbUser);
            }

            return Task.FromResult(Result.FromSuccess());
        }
    }
}