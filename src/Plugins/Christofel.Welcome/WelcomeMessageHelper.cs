//
//   WelcomeMessageHelper.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Welcome;

/// <summary>
/// Helper for creating welcome message.
/// </summary>
public class WelcomeMessageHelper
{
    /// <summary>
    /// Creates components for welcome message.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <returns>The message components.</returns>
    public static IMessageComponent[] CreateWelcomeComponents(WelcomeOptions options)
    {
        return new[]
        {
            new ActionRowComponent
            (
                new[]
                {
                    new ButtonComponent
                    (
                        ButtonComponentStyle.Primary,
                        options.WelcomeAuthButton,
                        CustomID: CustomIDHelpers.CreateButtonID("welcome auth")
                    ),
                    new ButtonComponent
                    (
                        ButtonComponentStyle.Secondary,
                        options.WelcomeEnglishButton,
                        CustomID: CustomIDHelpers.CreateButtonID("welcome english")
                    )
                }
            )
        };
    }
}