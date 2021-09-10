//
//   CtuAuthProcessData.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.User;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Core;

namespace Christofel.Api.Ctu
{
    /// <inheritdoc cref="Christofel.Api.Ctu.IAuthData"/>
    public record CtuAuthProcessData
        (
            string AccessToken,
            ILinkUser LoadedUser,
            Snowflake GuildId,
            ChristofelBaseContext DbContext,
            DbUser DbUser,
            IGuildMember GuildUser,
            CtuAuthAssignedRoles Roles
        )
        : IAuthData;

    /// <summary>
    /// Data used in ctu authentication process.
    /// </summary>
    public interface IAuthData
    {
        /// <summary>
        /// Gets access token that can be used to provide access to ctu services.
        /// </summary>
        [Obsolete("Use injection of authorized apis instead")]
        string AccessToken { get; }

        /// <summary>
        /// Gets loaded user from oauth check token.
        /// </summary>
        ILinkUser LoadedUser { get; }

        /// <summary>
        /// Gets id of the guild where the user is located.
        /// </summary>
        Snowflake GuildId { get; }

        /// <summary>
        /// Gets christofel base database context.
        /// </summary>
        /// <remarks>
        /// <see cref="DbUser"/> is loade in this context.
        /// </remarks>
        ChristofelBaseContext DbContext { get; }

        /// <summary>
        /// Gets user stored in the database.
        /// </summary>
        /// <remarks>
        /// Can be edited during the process.
        /// </remarks>
        DbUser DbUser { get; }

        /// <summary>
        /// Gets guild user is in.
        /// </summary>
        IGuildMember GuildUser { get; }

        /// <summary>
        /// Gets roles that should be assigned and removed at the end of the process.
        /// </summary>
        CtuAuthAssignedRoles Roles { get; }
    }
}