//
//   DuplicateAssignStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Resolvers;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Steps
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
            var duplicate = await _duplicates.ResolveDuplicateAsync(data.LoadedUser, ct);
            if (duplicate.Type != DuplicityType.None && duplicate.User is null)
            {
                return new InvalidOperationError("User cannot be null for non-none duplicate");
            }

            if (duplicate.Type == DuplicityType.Both)
            {
                duplicate.User!.AuthenticatedAt = DateTime.Now;
                data.DbContext.Remove(data.DbUser);
            }
            else if (duplicate.Type != DuplicityType.None)
            {
                data.DbUser.DuplicitUserId = duplicate.User!.UserId;
            }

            return Result.FromSuccess();
        }
    }
}