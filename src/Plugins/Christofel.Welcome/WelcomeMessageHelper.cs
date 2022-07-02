//
//   WelcomeMessageHelper.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices.ComTypes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Interactivity;

namespace Christofel.Welcome;

/// <summary>
/// Helper for creating welcome message.
/// </summary>
public static class WelcomeMessageHelper
{
    /// <summary>
    /// Creates components for welcome message.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="language">The language.</param>
    /// <returns>The message components.</returns>
    public static IMessageComponent[] CreateWelcomeComponents
        (WelcomeOptions options, string language)
    {
        return new[]
        {
            new ActionRowComponent
            (
                new IMessageComponent[]
                    {
                        new ButtonComponent
                        (
                            ButtonComponentStyle.Primary,
                            options.Translations[language].AuthButtonLabel,
                            CustomID: Constants.ChristofelPrefix + "::welcome auth " + language,
                            Emoji: new PartialEmoji(Name: options.AuthButtonEmoji)
                        ),
                    }
                    .Concat
                    (
                        options.Translations
                            .Where(x => x.Key != language)
                            .Select
                            (
                                x => new ButtonComponent
                                (
                                    ButtonComponentStyle.Secondary,
                                    x.Value.ShowButtonLabel,
                                    CustomID: Constants.ChristofelPrefix + "::welcome show " + x.Key
                                )
                            )
                    )
                    .ToArray()
            )
        };
    }
}