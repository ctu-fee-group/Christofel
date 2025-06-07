//
//   CtuUser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.User;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    /// <summary>
    /// Ctu user implementation for testing purposes.
    /// </summary>
    /// <param name="UserId">Id of the user.</param>
    /// <param name="CtuUsername">CTU Username.</param>
    public record CtuUser
    (
        int UserId,
        string CtuUsername
    ) : ICtuUser;
}