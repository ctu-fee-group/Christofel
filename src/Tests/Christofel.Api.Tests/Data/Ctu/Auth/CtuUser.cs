//
//   CtuUser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.User;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    public record CtuUser
    (
        int UserId,
        string CtuUsername
    ) : ICtuUser;
}