//
//   DbUser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Christofel.BaseLib.Database.Models.Abstractions;
using Christofel.BaseLib.User;
using Remora.Discord.Core;

namespace Christofel.BaseLib.Database.Models
{
    /// <summary>
    ///     Database table that holds authenticated users
    ///     or users in auth process.
    /// </summary>
    public class DbUser : ITimestampsEntity, IDiscordUser, IUser
    {
        /// <summary>
        ///     Last date of authentication
        /// </summary>
        public DateTime? AuthenticatedAt { get; set; }

        /// <summary>
        ///     CTU account username
        /// </summary>
        [MaxLength(256)]
        public string? CtuUsername { get; set; }

        /// <summary>
        ///     When this user is a duplicity (DuplicitUser is not null)
        ///     then set this to true if this user is allowed to finish the auth process
        /// </summary>
        public bool DuplicityApproved { get; set; }

        /// <summary>
        ///     Id of the user this is a duplicity with
        /// </summary>
        public int? DuplicitUserId { get; set; }

        /// <summary>
        ///     Code used for registration purposes
        /// </summary>
        public string? RegistrationCode { get; set; }

        public DbUser? DuplicitUser { get; set; }
        public List<DbUser>? DuplicitUsersBack { get; set; }

        [Key] public int UserId { get; set; }

        /// <summary>
        ///     Id of the user on Discord
        /// </summary>
        public Snowflake DiscordId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}