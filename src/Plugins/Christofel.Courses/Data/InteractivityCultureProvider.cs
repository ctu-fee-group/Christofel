//
//  InteractivityCultureProvider.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Helpers.Localization;
using Microsoft.Extensions.Options;

namespace Christofel.Courses.Data;

/// <summary>
/// Provides culture for interactivity and commands.
/// </summary>
public class InteractivityCultureProvider : ICultureProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InteractivityCultureProvider"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    public InteractivityCultureProvider(IOptionsSnapshot<LocalizationOptions> options)
    {
        CurrentCulture = options.Value.DefaultLanguage;
    }

    /// <inheritdoc/>
    public string CurrentCulture { get; set; }
}