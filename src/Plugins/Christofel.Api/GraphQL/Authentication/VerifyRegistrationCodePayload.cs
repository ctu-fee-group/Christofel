//
//   VerifyRegistrationCodePayload.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Christofel.Api.GraphQL.Common;

namespace Christofel.Api.GraphQL.Authentication
{
    /// <summary>
    /// Result of verifyRegistration mutation.
    /// </summary>
    public class VerifyRegistrationCodePayload : Payload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerifyRegistrationCodePayload"/> class.
        /// </summary>
        /// <param name="verificationStage">The verification stage.</param>
        public VerifyRegistrationCodePayload(RegistrationCodeVerification verificationStage)
            : base(new List<UserError>())
        {
            VerificationStage = verificationStage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifyRegistrationCodePayload"/> class.
        /// </summary>
        /// <param name="errors">The collection of errors that happened.</param>
        public VerifyRegistrationCodePayload(ICollection<UserError> errors)
            : base(errors)
        {
        }

        /// <summary>
        /// What step of the registration should be used.
        /// </summary>
        public RegistrationCodeVerification VerificationStage { get; }
    }
}