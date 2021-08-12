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

                KosProgramme? programme = await kosApi.LoadEntityAsync(student.Programme);

                int year;
                if (programme is null)
                {
                    year = student.StartDate.Year;
                }
                else
                {
                    year = await ObtainFurthestStudentSameType(
                        kosApi,
                        student,
                        kosPerson?.Roles?.Students?.Skip(1),
                        programme.ProgrammeType
                    );
                }

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

        private async Task<int> ObtainFurthestStudentSameType(AuthorizedKosApi kosApi, KosStudent student,
            IEnumerable<KosLoadableEntity<KosStudent>>? studentLoadables, KosProgrammeType programmeType)
        {
            int minYear = student.StartDate.Year;

            if (studentLoadables is null)
            {
                return minYear;
            }
            
            foreach (KosLoadableEntity<KosStudent> studentLoadable in studentLoadables)
            {
                KosStudent? currentStudent = await kosApi.LoadEntityAsync(studentLoadable);

                if (currentStudent is null)
                {
                    continue;
                }

                KosProgramme? programme = await kosApi.LoadEntityAsync(currentStudent.Programme);
                if (programme is null)
                {
                    continue;
                }
                
                if (programme.ProgrammeType == programmeType && currentStudent.StartDate.Year < minYear)
                {
                    minYear = currentStudent.StartDate.Year;
                }
            }

            return minYear;
        }
    }
}