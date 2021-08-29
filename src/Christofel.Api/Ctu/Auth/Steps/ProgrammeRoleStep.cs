using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kos;
using Kos.Abstractions;
using Kos.Atom;
using Kos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Steps
{
    /// <summary>
    /// Assign roles from ProgrammeRoleAssignment table
    /// </summary>
    /// <remarks>
    /// Uses kos api to obtain programme of the user
    /// </remarks>
    public class ProgrammeRoleStep : IAuthStep
    {
        private readonly IKosPeopleApi _kosPeopleApi;
        private readonly IKosAtomApi _kosApi;
        private readonly ILogger _logger;

        public ProgrammeRoleStep(ILogger<CtuAuthProcess> logger, IKosPeopleApi kosPeopleApi, IKosAtomApi kosApi)
        {
            _kosPeopleApi = kosPeopleApi;
            _logger = logger;
        }

        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            KosPerson? kosPerson =
                await _kosPeopleApi.GetPersonAsync(data.LoadedUser.CtuUsername, token: ct);

            AtomLoadableEntity<KosStudent>? studentLoadable = kosPerson?.Roles.Students.LastOrDefault();
            if (studentLoadable is not null)
            {
                KosStudent? student = await _kosApi.LoadEntityAsync(studentLoadable, token: ct);

                if (student is null)
                {
                    return Result.FromSuccess();
                }

                string? programmeTitle = student.Programme?.Title;

                if (programmeTitle is null)
                {
                    return Result.FromSuccess();
                }

                List<CtuAuthRole> roles = await data.DbContext.ProgrammeRoleAssignments
                    .AsNoTracking()
                    .Where(x => x.Programme == programmeTitle)
                    .Include(x => x.Assignment)
                    .Select(x => new CtuAuthRole
                    {
                        RoleId = x.Assignment.RoleId,
                        Type = x.Assignment.RoleType
                    })
                    .ToListAsync(ct);

                if (roles.Count == 0)
                {
                    _logger.LogWarning(
                        $"Could not find mapping for programme {programmeTitle} for user {data.GuildUser}");
                }

                data.Roles.AddRange(roles);
            }

            return Result.FromSuccess();
        }
    }
}