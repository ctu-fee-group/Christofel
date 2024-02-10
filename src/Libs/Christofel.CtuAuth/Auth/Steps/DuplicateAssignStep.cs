//
//   DuplicateAssignStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CtuAuth.Resolvers;
using Remora.Results;

namespace Christofel.CtuAuth.Auth.Steps
{
    /// <summary>
    /// Step that assigns correct information about duplicate.
    /// </summary>
    /// <remarks>
    /// If the duplicate type is both, original user will be modified and the new one will be removed.
    /// </remarks>
    public class DuplicateAssignStep : IAuthStep
    {
        private readonly DuplicateResolver _duplicates;

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateAssignStep"/> class.
        /// </summary>
        /// <param name="duplicates">The resolver of duplicates.</param>
        public DuplicateAssignStep(DuplicateResolver duplicates)
        {
            _duplicates = duplicates;
        }

        /// <inheritdoc />
        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            var duplicates = await _duplicates.ResolveDuplicateAsync(data.LoadedUser, ct);

            if (duplicates.Both is not null)
            {
                var duplicateUser = duplicates.Both.Users.First();
                var dbDuplicateUser = data.DbContext.Users.First(x => x.UserId == duplicateUser.UserId);
                dbDuplicateUser.AuthenticatedAt = DateTime.Now;
                data.DbContext.Remove(data.DbUser);
            }

            if (data.DbUser.DuplicityApproved)
            {
                return Result.FromSuccess();
            }

            var discordDuplicates = duplicates.Discord?.Users ?? Array.Empty<DuplicateUser>();
            var ctuDuplicates = duplicates.Ctu?.Users ?? Array.Empty<DuplicateUser>();

            foreach (var duplicate in discordDuplicates.Concat(ctuDuplicates))
            {
                var dbDuplicateUser = data.DbContext.Users.FirstOrDefault(x => x.UserId == duplicate.UserId);
                if (dbDuplicateUser is not null)
                {
                    dbDuplicateUser.AuthenticatedAt = null;
                }
            }

            return Result.FromSuccess();
        }
    }
}