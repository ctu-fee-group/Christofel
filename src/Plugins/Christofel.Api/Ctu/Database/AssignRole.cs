//
//   AssignRole.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using Remora.Discord.Core;

namespace Christofel.Api.Ctu.Database
{
    /// <summary>
    ///     Role to be assigned to user
    /// </summary>
    public class AssignRole
    {
        /// <summary>
        ///     Primary key
        /// </summary>
        [Key]
        public long AssignRoleId { get; set; }

        /// <summary>
        ///     Id of the user to assign the role to
        /// </summary>
        public Snowflake UserDiscordId { get; set; }

        /// <summary>
        ///     Id of the guild the user is located in
        /// </summary>
        public Snowflake GuildDiscordId { get; set; }

        /// <summary>
        ///     Id of the role to be added/removed
        /// </summary>
        public Snowflake RoleId { get; set; }

        /// <summary>
        ///     Add if true, Remove if false
        /// </summary>
        public bool Add { get; set; }
    }
}