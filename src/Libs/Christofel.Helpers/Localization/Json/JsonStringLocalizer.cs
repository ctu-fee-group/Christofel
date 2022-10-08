//
//  JsonStringLocalizer.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Christofel.Helpers.Localization.Json;

/// <inheritdoc />
public class JsonStringLocalizer : IStringLocalizer
{
    private readonly JsonResourceManager _resourceManager;
    private readonly string _resourceName;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonStringLocalizer"/> class.
    /// </summary>
    /// <param name="resourceManager">The json resource manager.</param>
    /// <param name="resourceName">The name of the resource.</param>
    public JsonStringLocalizer(JsonResourceManager resourceManager, string resourceName)
    {
        _resourceManager = resourceManager;
        _resourceName = resourceName;
    }

    /// <inheritdoc />
    public string Translate(string name, string? cultureName, params string[] parameters)
        => _resourceManager.Translate(_resourceName, name, cultureName, parameters);

    /// <inheritdoc />
    public string Translate(string name, string? cultureName, Dictionary<string, string> parameters)
        => _resourceManager.Translate(_resourceName, name, cultureName, parameters);
}