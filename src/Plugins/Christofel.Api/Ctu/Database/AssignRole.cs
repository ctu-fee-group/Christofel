//
//   AssignRole.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Christofel.Api.Ctu.Database
{
    /// <summary>
    /// Database table representing role to be assigned to user.
    /// </summary>
    [Table("AssignRole", Schema = ApiCacheContext.SchemaName)]
    public class AssignRole
    {
        /// <summary>
        /// Gets or sets primary key of <see cref="AssignRole"/>.
        /// </summary>
        [Key]
        public long AssignRoleId { get; set; }

        /// <summary>
        /// Gets or sets id of the user to assign the role to.
        /// </summary>
        public Snowflake UserDiscordId { get; set; }

        /// <summary>
        /// Gets or sets id of the guild the user is located in.
        /// </summary>
        public Snowflake GuildDiscordId { get; set; }

        /// <summary>
        /// Gets or sets id of the role to be added/removed.
        /// </summary>
        public Snowflake RoleId { get; set; }

        /// <summary>
        /// Gets or sets whether the user should be added or removed.
        /// </summary>
        /// <remarks>
        /// Add if true, Remove if false.
        /// </remarks>
        public bool Add { get; set; }
    }
}