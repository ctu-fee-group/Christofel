using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kos;
using Kos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Results;
using Usermap;
using Usermap.Data;

namespace Christofel.Api.Ctu.Auth.Steps
{
    /// <summary>
    /// Assign roles from TitleRoleAssignment table
    /// </summary>
    /// <remarks>
    /// Obtains titles either from kos (safer as there are titlesPre and titlesPost fields)
    /// or from usermap if kos user is not found (not safe, because the titles have to be parsed from full name)
    /// </remarks>
    public class TitlesRoleStep : IAuthStep
    {
        private record Titles(IEnumerable<string> Pre, IEnumerable<string> Post);

        private readonly AuthorizedUsermapApi _usermapApi;
        private readonly AuthorizedKosApi _kosApi;

        public TitlesRoleStep(AuthorizedUsermapApi usermapApi, AuthorizedKosApi kosApi)
        {
            _kosApi = kosApi;
            _usermapApi = usermapApi;
        }
        
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            Titles? titles = await GetKosTitles(data.LoadedUser.CtuUsername, ct) ??
                             await GetUsermapTitles(data.LoadedUser.CtuUsername, ct);

            if (titles is null)
            {
                return Result.FromSuccess();
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
                .ToListAsync(ct);

            data.Roles.AddRange(roles);
            return Result.FromSuccess();
        }

        private async Task<Titles?> GetKosTitles(string username, CancellationToken token)
        {
            KosPerson? person = await _kosApi.People.GetPersonAsync(username, token: token);
            return CreateTitles(person?.TitlesPre, person?.TitlesPost);
        }

        private async Task<Titles?> GetUsermapTitles(string username, CancellationToken token)
        {
            UsermapPerson? person = await _usermapApi.People.GetPersonAsync(username, token: token);

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