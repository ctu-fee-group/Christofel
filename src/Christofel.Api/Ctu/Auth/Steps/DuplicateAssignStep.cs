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
            if (!data.StepData.TryGetValue("Duplicate", out var duplicateObj) || duplicateObj is null)
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
            else if (duplicate.Type != DuplicityType.None)
            {
                duplicate.User.DuplicitUserId = duplicate.User.UserId;
            }

            return Task.FromResult(Result.FromSuccess());
        }
    }
}