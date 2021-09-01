using System;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Remora.Discord.Core;

namespace Christofel.Api.Tests.Data.Ctu.Auth
{
    public static class ChristofelBaseContextDummyExtensions
    {
        public static async Task<DbUser> SetupUserToAuthenticateAsync(this ChristofelBaseContext ctx)
        {
            var user = new DbUser()
            {
                UserId = 123,
                CreatedAt = DateTime.Now,
                DiscordId = new Snowflake(12348671238986),
                RegistrationCode = "2345-234",
                AuthenticatedAt = null,
                CtuUsername = null,
                DuplicitUser = null,
                DuplicitUserId = null,
                DuplicitUsersBack = null,
                DuplicityApproved = false,
                UpdatedAt = null
            };

            ctx.Add(user);
            await ctx.SaveChangesAsync();

            return user;
        }
    }
}