//
//   StringLocalizerExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Christofel.Courses.Extensions;

/// <summary>
/// Extension methods for <see cref="IStringLocalizer"/>.
/// </summary>
public static class StringLocalizerExtensions
{
    /// <summary>
    /// Translate the given resource to the given language/culture.
    /// </summary>
    /// <param name="localizer">The localizer.</param>
    /// <param name="name">The name of the string resource.</param>
    /// <param name="culture">The culture to translate to.</param>
    /// <param name="parameters">The parameters of the string resource.</param>
    /// <returns>The translated string.</returns>
    public static string Translate(this IStringLocalizer localizer, string name, string culture, params object[] parameters)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        var translated = localizer.GetString(name, parameters);
        CultureInfo.CurrentCulture = currentCulture;

        return translated;
    }
}