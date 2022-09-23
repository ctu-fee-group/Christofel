//
//   LinkUser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database.Models;
using Christofel.Common.User;
using Remora.Rest.Core;

namespace Christofel.Courses.Data;

public record LinkUser(string CtuUsername, Snowflake DiscordId) : ILinkUser
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LinkUser"/> class.
    /// </summary>
    /// <param name="dbUser">The Database user to create link user from.</param>
    public LinkUser(DbUser dbUser)
        : this(dbUser.CtuUsername!, dbUser.DiscordId)
    {
        if (dbUser.CtuUsername is null)
        {
            throw new ArgumentNullException(nameof(dbUser), "CtuUsername is null");
        }
    }
}