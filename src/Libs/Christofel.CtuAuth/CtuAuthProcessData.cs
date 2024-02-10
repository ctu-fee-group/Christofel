//
//   CtuAuthProcessData.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Common.User;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;

namespace Christofel.CtuAuth
{
    /// <inheritdoc cref="IAuthData"/>
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
        : IAuthData
    {
        private readonly List<ILinkUser> _linkedAccounts = new List<ILinkUser>();

        /// <inheritdoc />
        public IReadOnlyList<ILinkUser> UnapprovedLinkedAccounts => _linkedAccounts.AsReadOnly();

        /// <inheritdoc />
        public void AddLinkedAccount(ILinkUser user)
        {
            if (!_linkedAccounts.Contains(user))
            {
                _linkedAccounts.Add(user);
            }
        }
    }

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

        /// <summary>
        /// Gets the accounts linked with the data that are unapproved.
        /// </summary>
        IReadOnlyList<ILinkUser> UnapprovedLinkedAccounts { get; }

        /// <summary>
        /// Add a new linked account.
        /// </summary>
        /// <param name="user">The linked account.</param>
        void AddLinkedAccount(ILinkUser user);
    }
}