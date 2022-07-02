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
    /// Gets or sets the default language to send the welcome message as.
    /// </summary>
    public string DefaultLanguage { get; set; } = "en";

    /// <summary>
    /// Gets or sets the emoji for auth button.
    /// </summary>
    public string AuthButtonEmoji { get; set; } = "🔓";

    /// <summary>
    /// Gets or sets the dictionary with language translations.
    /// </summary>
    public IDictionary<string, WelcomeTranslationOptions> Translations { get; set; } = new Dictionary<string, WelcomeTranslationOptions>();

    /// <summary>
    /// Holds labels for translations.
    /// </summary>
    public class WelcomeTranslationOptions
    {
        /// <summary>
        /// Gets or sets the authentication button text.
        /// </summary>
        public string AuthButtonLabel { get; set; } = "Authenticate";

        /// <summary>
        /// Gets or sets the show english version text.
        /// </summary>
        public string ShowButtonLabel { get; set; } = "Show english version";

        /// <summary>
        /// Gets or sets the welcome embed json.
        /// </summary>
        public string EmbedFilePath { get; set; } = "Embeds/welcome.json";

        /// <summary>
        /// Gets or sets the authentication message.
        /// </summary>
        public string AuthMessage { get; set; } = "You can authenticate using: {Link}";
    }
}