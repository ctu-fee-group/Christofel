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
    /// Assign roles from YearRoleAssignments table
    /// </summary>
    /// <remarks>
    /// Obtains year of the start from kos, tries to find matching entry in database
    ///
    /// If there are more student records in the record, earliest one of the same type will be
    /// obtained.
    /// </remarks>
    public class YearRoleStep : IAuthStep
    {
        private readonly IKosPeopleApi _kosPeopleApi;
        private readonly IKosAtomApi _kosApi;
        private readonly ILogger _logger;

        public YearRoleStep(ILogger<CtuAuthProcess> logger, IKosPeopleApi kosPeopleApi, IKosAtomApi kosApi)
        {
            _kosApi = kosApi;
            _kosPeopleApi = kosPeopleApi;
            _logger = logger;
        }
        
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            KosPerson? kosPerson = await _kosPeopleApi.GetPersonAsync(data.LoadedUser.CtuUsername, token: ct);

            AtomLoadableEntity<KosStudent>? studentLoadable = kosPerson?.Roles.Students.FirstOrDefault();
            if (studentLoadable is not null)
            {
                KosStudent? student = await _kosApi.LoadEntityAsync(studentLoadable, token: ct);
                if (student is null)
                {
                    return Result.FromSuccess();
                }
                
                int year = student.StartDate.Year; 
                // First student is the one with lowest date (to not make so many requests, this is sufficient)

                List<CtuAuthRole> roles = await data.DbContext.YearRoleAssignments
                    .AsNoTracking()
                    .Where(x => x.Year == year)
                    .Include(x => x.Assignment)
                    .Select(x => new CtuAuthRole
                    {
                        RoleId = x.Assignment.RoleId,
                        Type = x.Assignment.RoleType
                    })
                    .ToListAsync(ct);

                if (roles.Count == 0)
                {
                    _logger.LogWarning($"Could not find mapping for year {year}");
                }

                data.Roles.AddRange(roles);
            }
            
            return Result.FromSuccess();
        }
    }
}