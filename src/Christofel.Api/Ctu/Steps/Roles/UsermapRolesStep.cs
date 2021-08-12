using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Usermap;
using Usermap.Data;

namespace Christofel.Api.Ctu.Steps.Roles
{
    public class UsermapRolesStep : CtuAuthStep
    {
        private readonly UsermapApi _usermapApi;

        public UsermapRolesStep(ILogger<CtuAuthProcess> logger, UsermapApi usermapApi)
            : base(logger)
        {
            _usermapApi = usermapApi;
        }

        protected override async Task<bool> HandleStep(CtuAuthProcessData data)
        {
            AuthorizedUsermapApi usermapApi = _usermapApi.GetAuthorizedApi(data.AccessToken);
            if (data.DbUser.CtuUsername is null)
            {
                throw new InvalidOperationException("CtuUsername is null");
            }

            UsermapPerson? person =
                await usermapApi.People.GetPersonAsync(data.DbUser.CtuUsername, data.CancellationToken);

            if (person is null)
            {
                return true;
            }

            List<CtuAuthRole> nonRegexRoleIds = await data.DbContext.UsermapRoleAssignments
                .AsNoTracking()
                .Where(x => !x.RegexMatch)
                .Where(x => person.Roles.Contains(x.UsermapRole))
                .Include(x => x.Assignment)
                .Select(x => new CtuAuthRole
                {
                    RoleId = x.Assignment.RoleId,
                    Type = x.Assignment.RoleType
                })
                .ToListAsync();

            IEnumerable<CtuAuthRole> regexRoleIds = (await data.DbContext.UsermapRoleAssignments
                    .AsNoTracking()
                    .Where(x => x.RegexMatch)
                    .Include(x => x.Assignment)
                    .Select(x => new { x.Assignment.RoleId, x.UsermapRole, x.Assignment.RoleType })
                    .ToListAsync())
                .Where(databaseMatch =>
                    person.Roles.Any(personRole => Regex.IsMatch(personRole, databaseMatch.UsermapRole)))
                .Select(x => new CtuAuthRole
                {
                    RoleId = x.RoleId,
                    Type = x.RoleType
                });

            data.Roles.AddRange(nonRegexRoleIds);
            data.Roles.AddRange(regexRoleIds);

            return true;
        }
    }
}