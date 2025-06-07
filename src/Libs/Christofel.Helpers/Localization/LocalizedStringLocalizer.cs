//
//  LocalizedStringLocalizer.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.Helpers.Localization;

/// <summary>
/// A string localizer that uses <see cref="IStringLocalizer"/> along with <see cref="ICultureProvider"/> to obtain what culture to use.
/// </summary>
public class LocalizedStringLocalizer
{
    private readonly IStringLocalizer _localizer;
    private readonly ICultureProvider _cultureProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedStringLocalizer"/> class.
    /// </summary>
    /// <param name="localizer">The localizer.</param>
    /// <param name="cultureProvider">The culture provider.</param>
    public LocalizedStringLocalizer(IStringLocalizer localizer, ICultureProvider cultureProvider)
    {
        _localizer = localizer;
        _cultureProvider = cultureProvider;
    }

    /// <summary>
    /// Gets the delocalized localizer.
    /// </summary>
    public IStringLocalizer Delocalized => _localizer;

    /// <summary>
    /// Gets the culture of this provider.
    /// </summary>
    public string Culture => _cultureProvider.CurrentCulture;

    /// <summary>
    /// Translate the given resource into the specified culture or to fallback if not found.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="parameters">The parameters to the string.</param>
    /// <returns>A translated string.</returns>
    public string Translate(string name, params string[] parameters)
        => _localizer.Translate(name, _cultureProvider.CurrentCulture, parameters);

    /// <summary>
    /// Translate the given resource into the specified culture or to fallback if not found.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="parameters">The named parameters to the string.</param>
    /// <returns>A translated string.</returns>
    public string Translate(string name, Dictionary<string, string> parameters)
        => _localizer.Translate(name, _cultureProvider.CurrentCulture, parameters);
}

/// <summary>
/// A string localizer that uses <see cref="IStringLocalizer"/> along with <see cref="ICultureProvider"/> to obtain what culture to use.
/// </summary>
/// <typeparam name="T">The type representing the translation resource.</typeparam>
public class LocalizedStringLocalizer<T> : LocalizedStringLocalizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedStringLocalizer{T}"/> class.
    /// </summary>
    /// <param name="localizer">The localizer.</param>
    /// <param name="cultureProvider">The culture provider.</param>
    public LocalizedStringLocalizer(IStringLocalizer<T> localizer, ICultureProvider cultureProvider)
        : base(localizer, cultureProvider)
    {
    }
}