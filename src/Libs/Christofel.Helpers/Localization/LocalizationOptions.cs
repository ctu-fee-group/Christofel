//
//  LocalizationOptions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Helpers.Localization;

/// <summary>
/// Options for localization.
/// </summary>
public class LocalizationOptions
{
    /// <summary>
    /// Gets or sets the resources path.
    /// </summary>
    public string ResourcesPath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the default language of Christofel.
    /// </summary>
    public string DefaultLanguage { get; set; } = null!;

    /// <summary>
    /// Gets or sets all the supported languages.
    /// </summary>
    public string[] SupportedLanguages { get; set; } = null!;
}