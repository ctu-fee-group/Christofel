//
//   UserError.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.Api.GraphQL.Common
{
    /// <summary>
    /// Validation error
    /// </summary>
    /// <param name="Message">Default message in english for ease of use</param>
    /// <param name="ErrorCode">Code to be used for </param>
    public record UserError
    (
        string Message,
        UserErrorCode ErrorCode
    ) : ResultError(Message);

    /// <summary>
    /// Repository for <see cref="UserError"/>.
    /// </summary>
    public static class UserErrors
    {
        /// <summary>
        /// Gets <see cref="UserErrorCode.Unspecified"/> error.
        /// </summary>
        public static UserError Unspecified => new UserError("Unspecified error", UserErrorCode.Unspecified);

        /// <summary>
        /// Gets <see cref="UserErrorCode.RejectedDuplicateUser"/> error.
        /// </summary>
        public static UserError RejectedDuplicateUser => new UserError
        (
            "There is a duplicate user stored, contact administrators, if you want to proceed",
            UserErrorCode.RejectedDuplicateUser
        );

        /// <summary>
        /// Gets <see cref="UserErrorCode.UserNotInGuild"/> error.
        /// </summary>
        public static UserError UserNotInGuild => new UserError
        (
            "User you are trying to log in with is not on the Discord server. Are you sure you are logging in with the correct user?",
            UserErrorCode.UserNotInGuild
        );

        /// <summary>
        /// Gets <see cref="UserErrorCode.InvalidRegistrationCode"/> error.
        /// </summary>
        public static UserError InvalidRegistrationCode => new UserError
            ("Specified registration code is not valid", UserErrorCode.InvalidRegistrationCode);

        /// <summary>
        /// Gets <see cref="UserErrorCode.SoftAuthError"/> error.
        /// </summary>
        public static UserError SoftAuthError => new UserError
            ("There was an error while assigning Discord roles, nickname etc.", UserErrorCode.SoftAuthError);
    }
}