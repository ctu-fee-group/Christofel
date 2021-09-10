//
//   TitlesRoleStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kos.Abstractions;
using Microsoft.EntityFrameworkCore;
using Remora.Results;
using Usermap.Controllers;

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
        private readonly IKosPeopleApi _kosPeopleApi;

        private readonly IUsermapPeopleApi _usermapPeopleApi;

        public TitlesRoleStep(IUsermapPeopleApi usermapPeopleApi, IKosPeopleApi kosPeopleApi)
        {
            _kosPeopleApi = kosPeopleApi;
            _usermapPeopleApi = usermapPeopleApi;
        }

        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            var titles = await GetKosTitles(data.LoadedUser.CtuUsername, ct) ??
                         await GetUsermapTitles(data.LoadedUser.CtuUsername, ct);

            if (titles is null)
            {
                return Result.FromSuccess();
            }

            List<CtuAuthRole> roles = await data.DbContext.TitleRoleAssignment
                .AsNoTracking()
                .Where
                (
                    x => x.Pre && titles.Pre.Contains(x.Title) ||
                         x.Post && titles.Post.Contains(x.Title)
                )
                .Include(x => x.Assignment)
                .Select(x => new CtuAuthRole { RoleId = x.Assignment.RoleId, Type = x.Assignment.RoleType })
                .ToListAsync(ct);

            data.Roles.AddRange(roles);
            return Result.FromSuccess();
        }

        private async Task<Titles?> GetKosTitles(string username, CancellationToken token)
        {
            var person = await _kosPeopleApi.GetPersonAsync(username, token);
            return CreateTitles(person?.TitlesPre, person?.TitlesPost);
        }

        private async Task<Titles?> GetUsermapTitles(string username, CancellationToken token)
        {
            var person = await _usermapPeopleApi.GetPersonAsync(username, token);

            if (person?.FullName is null)
            {
                return null;
            }

            var firstNameIndex = person.FullName.IndexOf(person.FirstName, StringComparison.InvariantCulture);
            var lastNameIndex = person.FullName.LastIndexOf(person.LastName, StringComparison.InvariantCulture) +
                                person.LastName.Length;

            string titlesPre = firstNameIndex <= 0
                ? ""
                : person.FullName.Substring(0, firstNameIndex);
            string titlesPost = lastNameIndex >= person.FullName.Length
                ? ""
                : person.FullName.Substring(0, firstNameIndex);

            return CreateTitles(titlesPre, titlesPost);
        }

        private Titles CreateTitles(string? pre, string? post) => new Titles(ObtainTitles(pre), ObtainTitles(post));

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

        private record Titles(IEnumerable<string> Pre, IEnumerable<string> Post);
    }
}