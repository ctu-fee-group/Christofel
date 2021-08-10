using System.Collections.Generic;
using Christofel.Api.GraphQL.Common;

namespace Christofel.Api.GraphQL.Authentication
{
    public enum RegistrationCodeVerification
    {
        NotValid,
        DiscordAuthorized,
        CtuAuthorized,
        Done,
    }
    
    public class VerifyRegistrationCodePayload : Payload
    {
        public VerifyRegistrationCodePayload(RegistrationCodeVerification verificationStage)
            : base(new List<UserError>())
        {
            VerificationStage = verificationStage;
        }
        
        public VerifyRegistrationCodePayload(ICollection<UserError> errors) : base(errors)
        {
        }
        
        public RegistrationCodeVerification VerificationStage { get; }
    }
}