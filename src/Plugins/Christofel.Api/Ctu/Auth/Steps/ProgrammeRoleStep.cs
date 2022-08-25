//
//   ProgrammeRoleStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    /// Assign roles from ProgrammeRoleAssignment table.
    /// </summary>
    /// <remarks>
    /// Uses kos api to obtain programme of the user.
    /// </remarks>
    public class ProgrammeRoleStep : IAuthStep
    {
        private readonly IKosPeopleApi _kosPeopleApi;
        private readonly IKosStudentsApi _kosStudentsApi;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgrammeRoleStep"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="kosPeopleApi">The kos people api.</param>
        /// <param name="kosStudentsApi">The kos students api.</param>
        public ProgrammeRoleStep
            (ILogger<ProgrammeRoleStep> logger, IKosPeopleApi kosPeopleApi, IKosStudentsApi kosStudentsApi)
        {
            _kosPeopleApi = kosPeopleApi;
            _kosStudentsApi = kosStudentsApi;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            var kosPerson =
                await _kosPeopleApi.GetPersonAsync(data.LoadedUser.CtuUsername, ct);

            var student = await GetLastStudentRole(kosPerson?.Roles.Students, ct);

            if (student is null)
            {
                return Result.FromSuccess();
            }

            var programmeTitle = student.Programme?.Title;

            if (programmeTitle is null)
            {
                return Result.FromSuccess();
            }

            List<CtuAuthRole> roles = await data.DbContext.ProgrammeRoleAssignments
                .AsNoTracking()
                .Where(x => x.Programme == programmeTitle)
                .Include(x => x.Assignment)
                .Select(x => new CtuAuthRole { RoleId = x.Assignment.RoleId, Type = x.Assignment.RoleType })
                .ToListAsync(ct);

            if (roles.Count == 0)
            {
                _logger.LogWarning
                    ($"Could not find mapping for programme {programmeTitle} for user {data.GuildUser}");
            }

            data.Roles.AddRange(roles);

            return Result.FromSuccess();
        }

        private async Task<Student?> GetLastStudentRole
            (List<AtomLoadableEntity<Student>>? studentRoles, CancellationToken ct)
        {
            Student? latestStudent = null;
            DateTime? latestDate = null;

            if (studentRoles is null)
            {
                return null;
            }

            foreach (var studentRole in studentRoles)
            {
                var currentStudent = await _kosStudentsApi.GetStudent(studentRole, token: ct);

                if (currentStudent is null)
                {
                    continue;
                }

                var currentStudentStartDate = currentStudent.StartDate ?? DateTime.Today;
                if (latestDate is null || currentStudentStartDate > latestDate)
                {
                    latestStudent = currentStudent;
                    latestDate = currentStudentStartDate;
                }
            }

            return latestStudent;
        }
    }
}