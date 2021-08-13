using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kos;
using Kos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Usermap;
using Usermap.Data;

namespace Christofel.Api.Ctu.Steps.Roles
{
    /// <summary>
    /// Assign roles from TitleRoleAssignment table
    /// </summary>
    /// <remarks>
    /// Obtains titles either from kos (safer as there are titlesPre and titlesPost fields)
    /// or from usermap if kos user is not found (not safe, because the titles have to be parsed from full name)
    /// </remarks>
    public class TitlesRoleStep : CtuAuthStep
    {
        private record Titles(IEnumerable<string> Pre, IEnumerable<string> Post);

        private readonly UsermapApi _usermapApi;
        private readonly KosApi _kosApi;

        public TitlesRoleStep(ILogger<CtuAuthProcess> logger, UsermapApi usermapApi, KosApi kosApi) : base(logger)
        {
            _kosApi = kosApi;
            _usermapApi = usermapApi;
        }

        protected override async Task<bool> HandleStep(CtuAuthProcessData data)
        {
            if (data.DbUser.CtuUsername is null)
            {
                throw new InvalidOperationException("CtuUsername is null");
            }

            Titles? titles = await GetKosTitles(data.AccessToken, data.DbUser.CtuUsername, data.CancellationToken) ??
                             await GetUsermapTitles(data.AccessToken, data.DbUser.CtuUsername, data.CancellationToken);

            if (titles is null)
            {
                return true;
            }

            List<CtuAuthRole> roles = await data.DbContext.TitleRoleAssignment
                .AsNoTracking()
                .Where(x => (x.Pre && titles.Pre.Contains(x.Title) ||
                             (x.Post && titles.Post.Contains(x.Title))))
                .Include(x => x.Assignment)
                .Select(x => new CtuAuthRole
                {
                    RoleId = x.Assignment.RoleId,
                    Type = x.Assignment.RoleType
                })
                .ToListAsync();

            data.Roles.AddRange(roles);

            return true;
        }

        private async Task<Titles?> GetKosTitles(string accessToken, string username, CancellationToken token)
        {
            AuthorizedKosApi kosApi = _kosApi.GetAuthorizedApi(accessToken);
            KosPerson? person = await kosApi.People.GetPerson(username, token);

            return CreateTitles(person?.TitlesPre, person?.TitlesPost);
        }

        private async Task<Titles?> GetUsermapTitles(string accessToken, string username, CancellationToken token)
        {
            AuthorizedUsermapApi usermapApi = _usermapApi.GetAuthorizedApi(accessToken);
            UsermapPerson? person = await usermapApi.People.GetPersonAsync(username, token);

            if (person?.FullName is null)
            {
                return null;
            }

            int firstNameIndex = person.FullName.IndexOf(person.FirstName, StringComparison.InvariantCulture);
            int lastNameIndex = person.FullName.LastIndexOf(person.LastName, StringComparison.InvariantCulture) +
                                person.LastName.Length;

            string titlesPre = firstNameIndex <= 0 ? "" : person.FullName.Substring(0, firstNameIndex);
            string titlesPost = lastNameIndex >= person.FullName.Length
                ? ""
                : person.FullName.Substring(0, firstNameIndex);

            return CreateTitles(titlesPre, titlesPost);
        }

        private Titles CreateTitles(string? pre, string? post)
        {
            return new Titles(
                ObtainTitles(pre), ObtainTitles(post)
            );
        }

        private IEnumerable<string> ObtainTitles(string? titles)
        {
            if (titles == null)
            {
                return Enumerable.Empty<string>();
            }

            return titles
                .Trim()
                .Split(' ')
                .Select(x => x.Trim());
        }
    }
}