//
//   DbUserExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Christofel.Common.Database.Models;
using Christofel.Common.User;
using Remora.Rest.Core;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth;

/// <summary>
/// Extensions for DbUser.
/// </summary>
public static class DbUserExtensions
{
    private record LinkUser(Snowflake DiscordId, string CtuUsername) : ILinkUser;

    /// <summary>
    /// Forcibly convert a DbUser to ILinkUser.
    /// </summary>
    /// <param name="user">The user to convert.</param>
    /// <returns>A link user.</returns>
    public static ILinkUser ToLinkUser(this DbUser user)
    {
        if (user.CtuUsername is null)
        {
            throw new InvalidOperationException();
        }

        return new LinkUser(user.DiscordId, user.CtuUsername!);
    }
}
