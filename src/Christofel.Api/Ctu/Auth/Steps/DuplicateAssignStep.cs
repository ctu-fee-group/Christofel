using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Resolvers;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Steps
{
    public class DuplicateAssignStep : IAuthStep
    {
        private readonly DuplicateResolver _duplicates;
        
        public DuplicateAssignStep(DuplicateResolver duplicates)
        {
            _duplicates = duplicates;
        }
        
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            var duplicate =  await _duplicates.ResolveDuplicateAsync(data.LoadedUser, ct);
            if (duplicate.Type != DuplicityType.None && duplicate.User is null)
            {
                return new InvalidOperationError("User cannot be null for non-none duplicate");
            }

            if (duplicate.Type == DuplicityType.Both)
            {
                duplicate.User!.AuthenticatedAt = DateTime.Now;
                data.DbContext.Remove(data.DbUser);
            }
            else if (duplicate.Type != DuplicityType.None)
            {
                data.DbUser.DuplicitUserId = duplicate.User!.UserId;
            }

            return Result.FromSuccess();
        }
    }
}