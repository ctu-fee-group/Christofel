//
//   UsermapRolesStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Remora.Results;
using Usermap.Controllers;

namespace Christofel.Api.Ctu.Auth.Steps
{
    /// <summary>
    /// Assign roles from UsermapRoleAssignment table
    /// </summary>
    /// <remarks>
    /// Obtains usermap roles if possible, then tries to match them against the ones
    /// in database. If there are matches, they are added
    /// </remarks>
    public class UsermapRolesStep : IAuthStep
    {
        private readonly IUsermapPeopleApi _usermapPeopleApi;

        public UsermapRolesStep(IUsermapPeopleApi usermapPeopleApi)
        {
            _usermapPeopleApi = usermapPeopleApi;
        }

        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            var person =
                await _usermapPeopleApi.GetPersonAsync(data.LoadedUser.CtuUsername, ct);

            if (person is null)
            {
                return Result.FromSuccess();
            }

            List<CtuAuthRole> nonRegexRoleIds = await data.DbContext.UsermapRoleAssignments
                .AsNoTracking()
                .Where(x => !x.RegexMatch)
                .Where(x => person.Roles.Contains(x.UsermapRole))
                .Include(x => x.Assignment)
                .Select(x => new CtuAuthRole { RoleId = x.Assignment.RoleId, Type = x.Assignment.RoleType })
                .ToListAsync(ct);

            IEnumerable<CtuAuthRole> regexRoleIds = (await data.DbContext.UsermapRoleAssignments
                    .AsNoTracking()
                    .Where(x => x.RegexMatch)
                    .Include(x => x.Assignment)
                    .Select(x => new { x.Assignment.RoleId, x.UsermapRole, x.Assignment.RoleType })
                    .ToListAsync(ct))
                .Where
                (
                    databaseMatch =>
                        person.Roles.Any(personRole => Regex.IsMatch(personRole, databaseMatch.UsermapRole))
                )
                .Select(x => new CtuAuthRole { RoleId = x.RoleId, Type = x.RoleType });

            data.Roles.AddRange(nonRegexRoleIds);
            data.Roles.AddRange(regexRoleIds);
            return Result.FromSuccess();
        }
    }
}