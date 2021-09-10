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
    /// Result of registerDiscord mutation.
    /// </summary>
    public class RegisterDiscordPayload : Payload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterDiscordPayload"/> class.
        /// </summary>
        /// <param name="user">The user to be returned.</param>
        /// <param name="registrationCode">The registration code to be used for ctu authentication.</param>
        public RegisterDiscordPayload(DbUser user, string registrationCode)
            : base(new List<UserError>())
        {
            User = user;
            RegistrationCode = registrationCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterDiscordPayload"/> class.
        /// </summary>
        /// <param name="error">The error to be sent to the user.</param>
        public RegisterDiscordPayload(UserError error)
            : base(new List<UserError>(new[] { error }))
        {
            RegistrationCode = string.Empty;
        }

        /// <summary>
        /// Information about the user.
        /// Only userId, discordId and registrationCode are expected to be filled at this point.
        /// </summary>
        public DbUser? User { get; }

        /// <summary>
        /// Registration code used for the second step of the registration (registerCtu).
        /// </summary>
        public string RegistrationCode { get; }
    }
}