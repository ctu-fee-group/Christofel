//
//  StringLocalizer.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.Helpers.Localization;

/// <inheritdoc />
public class StringLocalizer<T> : IStringLocalizer<T>
{
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLocalizer{T}"/> class.
    /// </summary>
    /// <param name="factory">The string localizer factory.</param>
    public StringLocalizer(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create(typeof(T));
    }

    /// <inheritdoc />
    public string Translate(string name, string? cultureName, params string[] parameters)
        => _localizer.Translate(name, cultureName, parameters);

    /// <inheritdoc />
    public string Translate(string name, string? cultureName, Dictionary<string, string> parameters)
        => _localizer.Translate(name, cultureName, parameters);
}