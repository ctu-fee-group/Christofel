using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kos;
using Kos.Atom;
using Kos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Christofel.Api.Ctu.Steps.Roles
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
    public class YearRoleStep : CtuAuthStep
    {
        private readonly KosApi _kosApi;

        public YearRoleStep(ILogger<CtuAuthProcess> logger, KosApi kosApi)
            : base(logger)
        {
            _kosApi = kosApi;
        }

        protected override async Task<bool> HandleStep(CtuAuthProcessData data)
        {
            AuthorizedKosApi kosApi = _kosApi.GetAuthorizedApi(data.AccessToken);
            KosPerson? kosPerson = await kosApi.People.GetPersonAsync(data.DbUser.CtuUsername ??
                                                                 throw new InvalidOperationException(
                                                                     "CtuUsername is null"), token: data.CancellationToken);

            AtomLoadableEntity<KosStudent>? studentLoadable = kosPerson?.Roles.Students.FirstOrDefault();
            if (studentLoadable is not null)
            {
                KosStudent? student = await kosApi.LoadEntityAsync(studentLoadable);
                if (student is null)
                {
                    return true;
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
                    .ToListAsync();

                if (roles.Count == 0)
                {
                    _logger.LogWarning($"Could not find mapping for year {year}");
                }

                data.Roles.AddRange(roles);
            }


            return true;
        }
    }
}