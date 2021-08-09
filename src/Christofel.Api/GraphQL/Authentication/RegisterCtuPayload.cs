using System.Collections.Generic;
using Christofel.Api.GraphQL.Common;
using Christofel.BaseLib.Database.Models;

namespace Christofel.Api.GraphQL.Authentication
{
    public class RegisterCtuPayload : Payload
    {
        public RegisterCtuPayload(DbUser user)
            : base(new List<UserError>())
        {
            User = user;
        }

        public RegisterCtuPayload(UserError error)
            : base (new List<UserError>(new []{error}))
        {
        }
        
        public DbUser? User { get; }
    }
}