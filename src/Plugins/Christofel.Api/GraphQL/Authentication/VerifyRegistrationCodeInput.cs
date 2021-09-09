//
//   VerifyRegistrationCodeInput.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Api.GraphQL.Authentication
{
    /// <summary>
    ///     Input of verifyRegistration mutation
    /// </summary>
    /// <param name="RegistrationCode">Code to verify</param>
    public record VerifyRegistrationCodeInput
    (
        string RegistrationCode
    );
}