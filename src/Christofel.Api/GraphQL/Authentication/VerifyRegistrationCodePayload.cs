using System.Collections.Generic;
using Christofel.Api.GraphQL.Common;

namespace Christofel.Api.GraphQL.Authentication
{
    public enum RegistrationCodeVerification
    {
        /// <summary>
        /// Code was not found, use registerDiscord
        /// </summary>
        NotValid,
        /// <summary>
        /// Code was found and only discord was registered, use registerCtu
        /// </summary>
        DiscordAuthorized,
        /// <summary>
        /// Code was found and both discord and ctu were linked.
        /// The process was not finalized, maybe because of a duplicity.
        /// Use registerCtu
        /// </summary>
        CtuAuthorized,
        /// <summary>
        /// This code was already used for registration and the user was successfully authenticated.
        /// This typically should not be returned as codes are removed after authentication is done
        /// </summary>
        Done,
    }
    
    /// <summary>
    /// Result of verifyRegistration mutation
    /// </summary>
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
        
        /// <summary>
        /// What step of the registration should be used
        /// </summary>
        public RegistrationCodeVerification VerificationStage { get; }
    }
}