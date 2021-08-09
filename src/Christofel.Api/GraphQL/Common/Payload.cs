using System.Collections.Generic;

namespace Christofel.Api.GraphQL.Common
{
    public abstract class Payload
    {
        public Payload(ICollection<UserError> errors)
        {
            Errors = errors;
        }
        
        public ICollection<UserError> Errors { get; }
    }
}