//
//   WelcomeOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Welcome;

/// <summary>
/// Options for welcome plugin.
/// </summary>
public class WelcomeOptions
{
    /// <summary>
    /// Gets or sets the authentication button text.
    /// </summary>
    public string WelcomeAuthButton { get; set; } = "Authenticate";

    /// <summary>
    /// Gets or sets the show english version text.
    /// </summary>
    public string WelcomeEnglishButton { get; set; } = "Show english version";

    /// <summary>
    /// Gets or sets the welcome embed json.
    /// </summary>
    public string WelcomeEmbedFile { get; set; } = "Embeds/welcome.json";

    /// <summary>
    /// Gets or sets the english version embed.
    /// </summary>
    public string EnglishWelcomeEmbedFile { get; set; } = "Embeds/welcome.english.json";

    /// <summary>
    /// Gets or sets the authentication message.
    /// </summary>
    public string AuthMessage { get; set; } = "You can authenticate using: {Link}";
}