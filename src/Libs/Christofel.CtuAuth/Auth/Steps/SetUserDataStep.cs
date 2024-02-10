//
//   SetUserDataStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.CtuAuth.Auth.Steps
{
    /// <summary>
    /// RegistrationCode will be set to null, AuthenticatedAt will be set to now.
    /// </summary>
    public class SetUserDataStep : IAuthStep
    {
        /// <inheritdoc />
        public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            data.DbUser.AuthenticatedAt = DateTime.Now;
            data.DbUser.RegistrationCode = null;
            data.DbUser.CtuUsername = data.LoadedUser.CtuUsername;

            return Task.FromResult(Result.FromSuccess());
        }
    }
}