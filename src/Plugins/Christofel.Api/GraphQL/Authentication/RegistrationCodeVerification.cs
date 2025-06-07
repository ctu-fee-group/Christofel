//
//   RegistrationCodeVerification.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Api.GraphQL.Authentication
{
    /// <summary>
    /// The verification code stage.
    /// </summary>
    public enum RegistrationCodeVerification
    {
        /// <summary>
        /// Code was not found, use registerDiscord.
        /// </summary>
        NotValid,

        /// <summary>
        /// Code was found and only discord was registered, use registerCtu.
        /// </summary>
        DiscordAuthorized,

        /// <summary>
        /// Code was found and both discord and ctu were linked.
        /// The process was not finalized, maybe because of a duplicity.
        /// Use registerCtu.
        /// </summary>
        CtuAuthorized,

        /// <summary>
        /// This code was already used for registration and the user was successfully authenticated.
        /// This typically should not be returned as codes are removed after authentication is done.
        /// </summary>
        Done,
    }
}