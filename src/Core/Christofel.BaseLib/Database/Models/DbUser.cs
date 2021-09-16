//
//   DbUser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Christofel.BaseLib.Database.Models.Abstractions;
using Christofel.BaseLib.User;
using Remora.Discord.Core;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    /// Database table that holds authenticated users
    /// or users in auth process.
    /// </summary>
    [Table("User", Schema = ChristofelBaseContext.SchemaName)]
    public class DbUser : ITimestampsEntity, IDiscordUser
    {
        /// <summary>
        /// Gets or sets the primary key of <see cref="DbUser"/>.
        /// </summary>
        [Key]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets last date of authentication.
        /// </summary>
        public DateTime? AuthenticatedAt { get; set; }

        /// <summary>
        /// Gets or sets CTU account username.
        /// </summary>
        [MaxLength(256)]
        public string? CtuUsername { get; set; }

        /// <summary>
        /// Gets or sets whether the duplicity was approved.
        /// </summary>
        /// <remarks>
        /// Users that are a duplicity won't get approved until this is set to true.
        /// </remarks>
        public bool DuplicityApproved { get; set; }

        /// <summary>
        /// Gets or sets id of the user this is a duplicity with.
        /// </summary>
        public int? DuplicitUserId { get; set; }

        /// <summary>
        /// Gets or sets code used for registration purposes.
        /// </summary>
        public string? RegistrationCode { get; set; }

        /// <summary>
        /// Gets or sets the duplicate user.
        /// </summary>
        public DbUser? DuplicitUser { get; set; }

        /// <summary>
        /// Gets or sets duplicit users that target this user.
        /// </summary>
        public List<DbUser>? DuplicitUsersBack { get; set; }

        /// <inheritdoc />
        public Snowflake DiscordId { get; set; }

        /// <inheritdoc />
        public DateTime CreatedAt { get; set; }

        /// <inheritdoc />
        public DateTime? UpdatedAt { get; set; }
    }
}