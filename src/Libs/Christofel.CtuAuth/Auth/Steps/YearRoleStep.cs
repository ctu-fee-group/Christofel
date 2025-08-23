//
//   YearRoleStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CtuAuth.Extensions;
using Kos.Abstractions;
using Kos.Data;
using Kos.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.CtuAuth.Auth.Steps
{
    /// <summary>
    /// Assign roles from YearRoleAssignments table.
    /// </summary>
    /// <remarks>
    /// Obtains year of the start from kos, tries to find matching entry in database
    /// Only CTU FEE student roles are used. First role is always used,
    /// then all roles of same programmetype are obtained, and year roles for all of those
    /// are given to the user.
    /// </remarks>
    public class YearRoleStep : IAuthStep
    {
        private readonly IKosPeopleApi _kosPeopleApi;
        private readonly IKosAtomApi _kosApi;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="YearRoleStep"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="kosPeopleApi">The kos people api.</param>
        /// <param name="kosApi">The kos api.</param>
        public YearRoleStep(ILogger<YearRoleStep> logger, IKosPeopleApi kosPeopleApi, IKosAtomApi kosApi)
        {
            _kosPeopleApi = kosPeopleApi;
            _kosApi = kosApi;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            var kosPerson = await _kosPeopleApi.GetPersonAsync(data.LoadedUser.CtuUsername, ct);
            if (kosPerson?.Roles.Students is null)
            {
                return Result.FromSuccess();
            }

            // Get student roles that are under FEE faculty
            Student[] feeStudentRoles = await kosPerson.Roles.Students
                .ToAsyncEnumerable()
                .SelectAwaitWithCancellation(async (sl, ct) => await _kosApi.LoadEntityContentAsync(sl, token: ct))
                .Where(x => x is not null)
                .Select(x => x!)
                .Where(s => s.Faculty?.GetKey() == "13000") // TODO: configurable faculty code
                .ToArrayAsync(ct);

            var initialStudent = feeStudentRoles.MinBy(student => student.StartDate ?? DateTime.Now);
            if (initialStudent is null)
            {
                return Result.FromSuccess();
            }

            ProgrammeType? programmeType = null;

            if (initialStudent.Programme is not null)
            {
                var programme = await _kosApi.LoadEntityContentAsync(initialStudent.Programme, token: ct);
                programmeType = programme?.ProgrammeType;
            }

            Student[] studentRoles;
            if (programmeType is null)
            {
                studentRoles =
                    [initialStudent];
            }
            else
            {
                studentRoles = await feeStudentRoles
                    .ToAsyncEnumerable()
                    .WhereAwaitWithCancellation(async (student, ct) =>
                    {
                        if (student.Programme is null)
                        {
                            return false;
                        }

                        var programme = await _kosApi.LoadEntityContentAsync(student.Programme, token: ct);
                        return programme?.ProgrammeType == programmeType;
                    })
                    .ToArrayAsync(ct);
            }

            var years = studentRoles
                .Select(x => x.StartDate?.Year ?? 0)
                .Distinct()
                .ToArray();

            var roles = await data.DbContext.YearRoleAssignments
                .AsNoTracking()
                .Where(x => years.Contains(x.Year))
                .Include(x => x.Assignment)
                .Select(x => new CtuAuthRole { RoleId = x.Assignment.RoleId, Type = x.Assignment.RoleType })
                .ToListAsync(ct);

            if (roles.Count < years.Length)
            {
                _logger.LogWarning(
                    "Could not find mapping for some of those year(s): {Years}",
                    string.Join(", ", years));
            }

            data.Roles.AddRange(roles);

            return Result.FromSuccess();
        }
    }
}
