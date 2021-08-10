using System.Collections.Generic;

namespace Christofel.Api.GraphQL.Common
{
    public abstract class Payload
    {
        public Payload(ICollection<UserError> errors)
        {
            Errors = errors;
        }
        
        /// <summary>
        /// Validation errors in case that there are any
        /// </summary>
        public ICollection<UserError> Errors { get; }
    }
}