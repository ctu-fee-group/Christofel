using System.Collections.Generic;
using Christofel.Api.GraphQL.Common;
using Christofel.BaseLib.Database.Models;

namespace Christofel.Api.GraphQL.Authentication
{
    public class RegisterDiscordPayload : Payload
    {
        public RegisterDiscordPayload(DbUser user, string registrationCode)
            : base(new List<UserError>())
        {
            User = user;
            RegistrationCode = registrationCode;
        }

        public RegisterDiscordPayload(UserError error)
            : base (new List<UserError>(new []{error}))
        {
            RegistrationCode = "";
        }
        
        public DbUser? User { get; }
        
        public string RegistrationCode { get; }
    }
}