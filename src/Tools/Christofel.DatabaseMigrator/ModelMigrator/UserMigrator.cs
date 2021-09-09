using System;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.DatabaseMigrator.Model;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;

namespace Christofel.DatabaseMigrator.ModelMigrator
{
    public class UserMigrator : IModelMigrator
    {
        private readonly OldContext _oldContext;
        private readonly ChristofelBaseContext _baseContext;
        
        public UserMigrator(OldContext oldContext, ChristofelBaseContext baseContext)
        {
            _oldContext = oldContext;
            _baseContext = baseContext;
        }
        
        public async Task MigrateModel()
        {
            await foreach (var user in _oldContext.Users)
            {
                var clonedUser = new DbUser()
                {
                    AuthenticatedAt = user.Authorized ? DateTime.Now : null,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    DuplicityApproved = user.DuplicityApproved,
                    DiscordId = new Snowflake(ulong.Parse(user.DiscordId)),
                    CtuUsername = user.CtuUsername,
                    RegistrationCode = user.AuthCode
                };

                _baseContext.Add(clonedUser);
            }
            
            // TODO FIND duplicates?

            await _baseContext.SaveChangesAsync();
        }
    }
}