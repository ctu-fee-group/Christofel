//
//   ChristofelBaseContextDummyExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Remora.Rest.Core;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    /// <summary>
    /// Class containing extensions for <see cref="ChristofelBaseContext"/>.
    /// </summary>
    public static class ChristofelBaseContextDummyExtensions
    {
        /// <summary>
        /// Adds user with AuthenticatedAt set to now.
        /// </summary>
        /// <param name="ctx">The base database context.</param>
        /// <param name="username">The username to set to the user.</param>
        /// <param name="discordId">The discord id of the user.</param>
        /// <returns>User that was added to the context.</returns>
        public static async Task<DbUser> SetupUserToAuthenticateAsync
            (this ChristofelBaseContext ctx, string? username = null, ulong discordId = 12348671238986)
        {
            var user = new DbUser
            {
                CreatedAt = DateTime.Now,
                DiscordId = new Snowflake(discordId),
                RegistrationCode = "2345-234",
                AuthenticatedAt = null,
                CtuUsername = username,
                DuplicitUser = null,
                DuplicitUserId = null,
                DuplicitUsersBack = null,
                DuplicityApproved = false,
                UpdatedAt = null,
            };

            ctx.Add(user);
            await ctx.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Adds user with AuthenticatedAt set to null.
        /// </summary>
        /// <param name="ctx">The base database context.</param>
        /// <param name="username">The username to set to the user.</param>
        /// <param name="discordId">The discord id of the user.</param>
        /// <returns>User that was added to the context.</returns>
        public static async Task<DbUser> SetupAuthenticatedUserAsync
            (this ChristofelBaseContext ctx, string? username = null, ulong discordId = 12348671238986)
        {
            var user = new DbUser
            {
                CreatedAt = DateTime.Now,
                DiscordId = new Snowflake(discordId),
                RegistrationCode = "2345-234",
                AuthenticatedAt = DateTime.Now,
                CtuUsername = username,
                DuplicitUser = null,
                DuplicitUserId = null,
                DuplicitUsersBack = null,
                DuplicityApproved = false,
                UpdatedAt = null,
            };

            ctx.Add(user);
            await ctx.SaveChangesAsync();

            return user;
        }
    }
}