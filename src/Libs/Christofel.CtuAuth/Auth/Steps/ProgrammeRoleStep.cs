//
//   ProgrammeRoleStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CtuAuth.Extensions;
using Kos.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.CtuAuth.Auth.Steps
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
        private readonly IKosAtomApi _kosApi;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgrammeRoleStep"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="kosPeopleApi">The kos people api.</param>
        /// <param name="kosApi">The kos api.</param>
        public ProgrammeRoleStep
            (ILogger<ProgrammeRoleStep> logger, IKosPeopleApi kosPeopleApi, IKosAtomApi kosApi)
        {
            _kosPeopleApi = kosPeopleApi;
            _kosApi = kosApi;
            _kosApi = kosApi;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            var kosPerson =
                await _kosPeopleApi.GetPersonAsync(data.LoadedUser.CtuUsername, ct);

            var student = await _kosApi.GetLatestStudentRole(kosPerson?.Roles.Students, ct: ct);

            if (student is null)
            {
                return Result.FromSuccess();
            }

            var programmeTitle = student.Programme?.Title;

            if (programmeTitle is null)
            {
                return Result.FromSuccess();
            }

            var roles = await data.DbContext.ProgrammeRoleAssignments
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
    }
}