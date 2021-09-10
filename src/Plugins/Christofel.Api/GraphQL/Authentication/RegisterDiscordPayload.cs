//
//   RegisterDiscordPayload.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Christofel.Api.GraphQL.Common;
using Christofel.BaseLib.Database.Models;

namespace Christofel.Api.GraphQL.Authentication
{
    /// <summary>
    /// Result of registerDiscord mutation
    /// </summary>
    public class RegisterDiscordPayload : Payload
    {
        public RegisterDiscordPayload(DbUser user, string registrationCode)
            : base(new List<UserError>())
        {
            User = user;
            RegistrationCode = registrationCode;
        }

        public RegisterDiscordPayload(UserError error)
            : base(new List<UserError>(new[] { error }))
        {
            RegistrationCode = "";
        }

        /// <summary>
        /// Information about the user.
        /// Only userId, discordId and registrationCode are expected to be filled at this point
        /// </summary>
        public DbUser? User { get; }

        /// <summary>
        /// Registration code used for the second step of the registration (registerCtu)
        /// </summary>
        public string RegistrationCode { get; }
    }
}