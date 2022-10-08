//
//  IStringLocalizer.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.Helpers.Localization;

/// <inheritdoc />
public interface IStringLocalizer<out T> : IStringLocalizer
{
}

/// <summary>
/// Translates strings to given culture.
/// </summary>
public interface IStringLocalizer
{
    /// <summary>
    /// Translate the given resource into the specified culture or to fallback if not found.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="cultureName">The name of the culture to find.</param>
    /// <param name="parameters">The parameters to the string.</param>
    /// <returns>A translated string.</returns>
    public string Translate(string name, string? cultureName, params string[] parameters);

    /// <summary>
    /// Translate the given resource into the specified culture or to fallback if not found.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="cultureName">The name of the culture to find.</param>
    /// <param name="parameters">The named parameters to the string.</param>
    /// <returns>A translated string.</returns>
    public string Translate(string name, string? cultureName, Dictionary<string, string> parameters);
}