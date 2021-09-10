//
//   RegisterCtuInput.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Api.GraphQL.Authentication
{
    /// <summary>
    /// Input for registerCtu mutation.
    /// OauthCode is the code obtained from oauth2.
    /// Redirect uri is the one passed to oauth2.
    /// Registration code is obtained from the first step of the registration (registerDiscord).
    /// </summary>
    /// <param name="OauthCode">Code obtained from oauth2</param>
    /// <param name="RedirectUri">Redirect uri passed to oauth2</param>
    /// <param name="RegistrationCode">Code obtained from the first step of the registration (registerDiscord)</param>
    public record RegisterCtuInput
    (
        string OauthCode,
        string RedirectUri,
        string RegistrationCode
    );
}