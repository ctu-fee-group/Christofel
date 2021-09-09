//
//   UserError.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.Api.GraphQL.Common
{
    /// <summary>
    ///     Validation error
    /// </summary>
    /// <param name="Message">Default message in english for ease of use</param>
    /// <param name="ErrorCode">Code to be used for </param>
    public record UserError
    (
        string Message,
        UserErrorCode ErrorCode
    ) : ResultError(Message);

    public static class UserErrors
    {
        public static UserError Unspecified => new UserError("Unspecified error", UserErrorCode.Unspecified);

        public static UserError RejectedDuplicateUser => new UserError
        (
            "There is a duplicate user stored, contact administrators, if you want to proceed",
            UserErrorCode.Unspecified
        );

        public static UserError UserNotInGuild => new UserError
        (
            "User you are trying to log in with is not on the Discord server. Are you sure you are logging in with the correct user?",
            UserErrorCode.UserNotInGuild
        );

        public static UserError InvalidRegistrationCode => new UserError
            ("Specified registration code is not valid", UserErrorCode.InvalidRegistrationCode);
    }

    /// <summary>
    /// </summary>
    public enum UserErrorCode
    {
        /// <summary>
        ///     Error of unknown type that cannot be shown to the user
        /// </summary>
        Unspecified,

        /// <summary>
        ///     Token was rejected (or 500 from oauth handler)
        /// </summary>
        OauthTokenRejected,

        /// <summary>
        ///     User trying to authenticate is a rejected duplicate
        /// </summary>
        /// <remarks>
        ///     The user must contact the administrators in order to be allowed authentication
        /// </remarks>
        RejectedDuplicateUser,

        /// <summary>
        ///     The user trying to log in is not in the correct guild
        /// </summary>
        UserNotInGuild,

        /// <summary>
        ///     The given registration is not valid
        /// </summary>
        /// <remarks>
        ///     The code may have been already used successfully, it has expired or destroyed manually
        /// </remarks>
        InvalidRegistrationCode,
    }
}