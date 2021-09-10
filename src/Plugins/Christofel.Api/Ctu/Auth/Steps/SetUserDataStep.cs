//
//   SetUserDataStep.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Steps
{
    /// <summary>
    /// Last step of verification, should be reached only if there was not an error.
    /// RegistrationCode will be set to null, AuthenticatedAt will be set to now
    /// </summary>
    public class SetUserDataStep : IAuthStep
    {
        public Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            data.DbUser.AuthenticatedAt = DateTime.Now;
            data.DbUser.RegistrationCode = null;
            data.DbUser.CtuUsername = data.LoadedUser.CtuUsername;

            return Task.FromResult(Result.FromSuccess());
        }
    }
}