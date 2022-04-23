//
//   YearRoleStep.cs
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Steps
{
    /// <summary>
    /// Assign roles from YearRoleAssignments table.
    /// </summary>
    /// <remarks>
    /// Obtains year of the start from kos, tries to find matching entry in database
    /// If there are more student records in the record, earliest one of the same type will be.
    /// obtained.
    /// </remarks>
    public class YearRoleStep : IAuthStep
    {
        private readonly IKosPeopleApi _kosPeopleApi;
        private readonly IKosStudentsApi _kosStudentApi;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="YearRoleStep"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="kosPeopleApi">The kos people api.</param>
        /// <param name="kosStudentApi">The kos students api.</param>
        public YearRoleStep(ILogger<YearRoleStep> logger, IKosPeopleApi kosPeopleApi, IKosStudentsApi kosStudentApi)
        {
            _kosPeopleApi = kosPeopleApi;
            _kosStudentApi = kosStudentApi;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            var kosPerson = await _kosPeopleApi.GetPersonAsync(data.LoadedUser.CtuUsername, ct);

            // First student is the one with lowest date (to not make so many requests, this is sufficient)
            var studentLoadable = kosPerson?.Roles.Students.FirstOrDefault();
            if (studentLoadable is not null)
            {
                var student = await _kosStudentApi.GetStudent(studentLoadable, token: ct);
                if (student is null)
                {
                    return Result.FromSuccess();
                }

                var year = student.StartDate?.Year ?? 0;

                List<CtuAuthRole> roles = await data.DbContext.YearRoleAssignments
                    .AsNoTracking()
                    .Where(x => x.Year == year)
                    .Include(x => x.Assignment)
                    .Select(x => new CtuAuthRole { RoleId = x.Assignment.RoleId, Type = x.Assignment.RoleType })
                    .ToListAsync(ct);

                if (roles.Count == 0)
                {
                    _logger.LogWarning("Could not find mapping for year {Year}", year);
                }

                data.Roles.AddRange(roles);
            }

            return Result.FromSuccess();
        }
    }
}