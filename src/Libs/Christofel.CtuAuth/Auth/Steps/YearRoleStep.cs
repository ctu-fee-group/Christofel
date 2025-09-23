//
//   YearRoleStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CtuAuth.Auth.Tasks.Options;
using Christofel.CtuAuth.Extensions;
using Kos.Abstractions;
using Kos.Data;
using Kos.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IKosProgrammesApi _kosProgrammesApi;
        private readonly IKosAtomApi _kosApi;
        private readonly ILogger _logger;
        private readonly AuthOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="YearRoleStep"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="kosPeopleApi">The kos people api.</param>
        /// <param name="kosProgrammesApi">The kos programmes api.</param>
        /// <param name="kosApi">The kos api.</param>
        /// <param name="options">The options configuration for faculty.</param>
        public YearRoleStep
        (
            ILogger<YearRoleStep> logger,
            IKosPeopleApi kosPeopleApi,
            IKosProgrammesApi kosProgrammesApi,
            IKosAtomApi kosApi,
            IOptionsSnapshot<AuthOptions> options
        )
        {
            _kosPeopleApi = kosPeopleApi;
            _kosProgrammesApi = kosProgrammesApi;
            _kosApi = kosApi;
            _logger = logger;
            _options = options.Value;
        }

        /// <inheritdoc />
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            var kosPerson = await _kosPeopleApi.GetPersonAsync(data.LoadedUser.CtuUsername, ct);
            if (kosPerson?.Roles.Students is null)
            {
                return Result.FromSuccess();
            }

            if (_options.FacultyCode is null)
            {
                throw new InvalidOperationException("FacultyCode not supplied in config!");
            }

            // Get student roles that are under FEE faculty
            Student[] feeStudentRoles = await kosPerson.Roles.Students
                .ToAsyncEnumerable()
                .SelectAwaitWithCancellation(async (sl, ct) => await _kosApi.LoadEntityContentAsync(sl, token: ct))
                .Where(x => x is not null)
                .Select(x => x!)
                .Where(s => s.Faculty?.GetKey() == _options.FacultyCode)
                .ToArrayAsync(ct);

            var initialStudent = feeStudentRoles.MinBy(student => student.StartDate ?? DateTime.Now);
            if (initialStudent is null)
            {
                return Result.FromSuccess();
            }

            ProgrammeType? programmeType = null;

            if (initialStudent.Programme is not null)
            {
                var (programme, unique) = await _kosProgrammesApi
                    .GetNonUniqueProgramme(initialStudent.Programme, ct: ct);
                programmeType = programme?.ProgrammeType;

                if (!unique)
                {
                    _logger.LogWarning($"Programme {initialStudent.Programme?.Title} ({initialStudent.Programme!.GetKey()}) is not unique, assuming first one is the correct one.");
                }

                if (programme is null)
                {
                    _logger.LogWarning($"Programme {initialStudent.Programme?.Title} ({initialStudent.Programme!.GetKey()}) not found, assigning only first year role.");
                }
            }
            else
            {
                _logger.LogWarning($"Student doesn't have any programme, assigning only first year role.");
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

                        var (programme, unique) = await _kosProgrammesApi
                            .GetNonUniqueProgramme(student.Programme, ct: ct);

                        if (!unique)
                        {
                            _logger.LogWarning($"Programme {student.Programme?.Title} ({student.Programme!.GetKey()}) is not unique, assuming first one is the correct one.");
                        }

                        if (programme is null)
                        {
                            _logger.LogWarning($"Programme {student.Programme?.Title} ({student.Programme!.GetKey()}) not found, cannot determine if to assign year role, skipping. (the user might be missing {student.StartDate?.Year} role)");
                        }

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
