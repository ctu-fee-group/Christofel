using System.Collections.Generic;
using Christofel.Api.GraphQL.Common;
using Christofel.BaseLib.Database.Models;

namespace Christofel.Api.GraphQL.Authentication
{
    public class CtuRegisterPayload : Payload
    {
        public CtuRegisterPayload(DbUser user)
            : base(new List<UserError>())
        {
            User = user;
        }

        public CtuRegisterPayload(UserError error)
            : base (new List<UserError>(new []{error}))
        {
        }
        
        public DbUser? User { get; }
    }
}