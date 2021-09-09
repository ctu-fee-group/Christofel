//
//   ChristofelBaseContextDummyExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Remora.Discord.Core;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    public static class ChristofelBaseContextDummyExtensions
    {
        public static async Task<DbUser> SetupUserToAuthenticateAsync
            (this ChristofelBaseContext ctx, string username = null, ulong discordId = 12348671238986)
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

        public static async Task<DbUser> SetupAuthenticatedUserAsync
            (this ChristofelBaseContext ctx, string username = null, ulong discordId = 12348671238986)
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