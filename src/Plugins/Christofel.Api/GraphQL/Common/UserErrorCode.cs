//
//   UserErrorCode.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Api.GraphQL.Common
{
    /// <summary>
    /// Codes of <see cref="UserError"/> for distinguishing between different types of errors.
    /// </summary>
    public enum UserErrorCode
    {
        /// <summary>
        /// Error of unknown type that cannot be shown to the user.
        /// </summary>
        Unspecified,

        /// <summary>
        /// Token was rejected (or 500 from oauth handler).
        /// </summary>
        OauthTokenRejected,

        /// <summary>
        /// User trying to authenticate is a rejected duplicate.
        /// </summary>
        /// <remarks>
        /// The user must contact the administrators in order to be allowed authentication.
        /// </remarks>
        RejectedDuplicateUser,

        /// <summary>
        /// The user trying to log in is not in the correct guild.
        /// </summary>
        UserNotInGuild,

        /// <summary>
        /// The given registration code is not valid.
        /// </summary>
        /// <remarks>
        /// The code may have been already used successfully, it has expired or was destroyed manually.
        /// </remarks>
        InvalidRegistrationCode,
    }
}