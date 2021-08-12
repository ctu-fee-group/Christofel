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
    public class ProgrammeRoleStep : CtuAuthStep
    {
        private readonly KosApi _kosApi;

        public ProgrammeRoleStep(ILogger<CtuAuthProcess> logger, KosApi kosApi)
            : base(logger)
        {
            _kosApi = kosApi;
        }

        protected override async Task<bool> HandleStep(CtuAuthProcessData data)
        {
            // Support for obtaining programmes from usermap could be done, but would be difficult

            AuthorizedKosApi kosApi = _kosApi.GetAuthorizedApi(data.AccessToken);
            KosPerson? kosPerson = await kosApi.People.GetPerson(data.DbUser.CtuUsername ??
                                                             throw new InvalidOperationException(
                                                                 "CtuUsername is null"));

            KosLoadableEntity<KosStudent>? studentLoadable = kosPerson?.Roles.Students.FirstOrDefault();
            if (studentLoadable is not null)
            {
                KosStudent? student = await kosApi.LoadEntityAsync(studentLoadable);

                if (student is null)
                {
                    return true;
                }

                string? programmeTitle = student.Programme?.Title;

                if (programmeTitle is null)
                {
                    return true;
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
                    .ToListAsync();

                if (roles.Count == 0)
                {
                    _logger.LogWarning($"Could not find mapping for programme {programmeTitle} for user {data.GuildUser}");
                }
                
                data.Roles.AddRange(roles);
            }
            
            return true;
        }
    }
}