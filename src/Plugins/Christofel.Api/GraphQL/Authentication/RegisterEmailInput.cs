//
//  RegisterEmailInput.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Api.GraphQL.Authentication;

/// <summary>
/// Input for registerEmailCode mutation.
/// </summary>
/// <param name="EmailCode">Code obtained from the email.</param>
/// <param name="RegistrationCode">Code obtained from the first step of the registration (welcome interaction).</param>
public record RegisterEmailInput
(
    string EmailCode,
    string RegistrationCode
);