//
//  JsonResourceManager.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Christofel.Helpers.Localization.Json;

/// <summary>
/// Manages json files.
/// </summary>
public class JsonResourceManager
{
    private readonly LocalizationOptions _options;
    private readonly ILogger _logger;

    // resourceSource -> JsonResourceSource.
    private readonly Dictionary<string, JsonResourceSource> _sources;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonResourceManager"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    internal JsonResourceManager(LocalizationOptions options, ILogger logger)
    {
        _options = options;
        _logger = logger;
        _sources = new Dictionary<string, JsonResourceSource>();
    }

    /// <summary>
    /// Translate the given resource into the specified culture or to fallback if not found.
    /// </summary>
    /// <param name="resourceName">The name of the resource source.</param>
    /// <param name="name">The name of the entry.</param>
    /// <param name="cultureName">The name of the culture to find.</param>
    /// <param name="parameters">The parameters to the string.</param>
    /// <returns>A translated string.</returns>
    public string Translate
    (
        string resourceName,
        string name,
        string? cultureName,
        string[] parameters
    )
    {
        var rawTranslation = Translate(resourceName, name, cultureName);

        for (int i = 0; i < parameters.Length; i++)
        {
            rawTranslation = rawTranslation.Replace($"{i}", parameters[i]);
        }

        return rawTranslation;
    }

    /// <summary>
    /// Translate the given resource into the specified culture or to fallback if not found.
    /// </summary>
    /// <param name="resourceName">The name of the resource source.</param>
    /// <param name="name">The name of the entry.</param>
    /// <param name="cultureName">The name of the culture to find.</param>
    /// <param name="parameters">The named parameters to the string.</param>
    /// <returns>A translated string.</returns>
    public string Translate
    (
        string resourceName,
        string name,
        string? cultureName,
        Dictionary<string, string> parameters
    )
    {
        var rawTranslation = Translate(resourceName, name, cultureName);

        foreach (var parameter in parameters)
        {
            rawTranslation = rawTranslation.Replace($"{parameter.Key}", parameter.Value);
        }

        return rawTranslation;
    }

    private string Translate(string resourceName, string name, string? cultureName)
    {
        var source = Load(resourceName);

        if (cultureName is null)
        {
            cultureName = "default";
        }

        if (!source.ContainsCulture(cultureName))
        {
            var fallbackPath = GetFilePath(resourceName, cultureName);
            source.AddCulture
            (
                cultureName,
                new JsonResourceTranslations(ObtainTranslations(resourceName, cultureName, fallbackPath))
            );
        }

        var translations = source.GetCulture(cultureName);
        var translation = translations.GetTranslation(name);

        if (translation is null)
        {
            translations = source.GetCulture("default");
            translation = translations.GetTranslation(name);
        }

        if (translation is null)
        {
            translation = name;
        }

        return translation;
    }

    private JsonResourceSource Load(string resource)
    {
        if (_sources.ContainsKey(resource))
        {
            return _sources[resource];
        }

        var fallbackPath = GetFallbackFilePath(resource);
        var loadedFallbackTranslations = ObtainTranslations(resource, "default", fallbackPath);

        var resourceSource = new JsonResourceSource();
        resourceSource.AddCulture("default", new JsonResourceTranslations(loadedFallbackTranslations));

        _sources[resource] = resourceSource;
        return resourceSource;
    }

    private string GetFilePath(string resourceSource, string cultureName)
    {
        return Path.Join(_options.ResourcesPath, resourceSource + $".{cultureName}.json");
    }

    private string GetFallbackFilePath(string resourceSource)
    {
        return Path.Join(_options.ResourcesPath, resourceSource + ".json");
    }

    private Dictionary<string, string> ObtainTranslations
    (
        string resourceSource,
        string cultureName,
        string filePath
    )
    {
        if (!File.Exists(filePath))
        {
            _logger.LogDebug
            (
                "Could not find translation file for {resourceSource}, culture {culture}.",
                resourceSource,
                cultureName
            );
            return new Dictionary<string, string>();
        }

        var deserialized = JsonSerializer.Deserialize<JsonResource[]>(File.ReadAllText(filePath));
        if (deserialized is null)
        {
            return new Dictionary<string, string>();
        }

        var translations = new Dictionary<string, string>();
        foreach (var resource in deserialized)
        {
            translations[resource.Name] = resource.Value;
        }

        return translations;
    }

    private class JsonResourceSource
    {
        // CultureName -> JsonResourceTranslations
        private readonly Dictionary<string, JsonResourceTranslations> _cultures;

        public JsonResourceSource()
        {
            _cultures = new Dictionary<string, JsonResourceTranslations>();
        }

        public JsonResourceTranslations GetCulture(string cultureName)
        {
            return _cultures[cultureName];
        }

        public bool ContainsCulture(string cultureName)
        {
            return _cultures.ContainsKey(cultureName);
        }

        public void AddCulture(string cultureName, JsonResourceTranslations translations)
        {
            _cultures[cultureName] = translations;
        }
    }

    private class JsonResourceTranslations
    {
        private readonly Dictionary<string, string> _translations;

        public JsonResourceTranslations(Dictionary<string, string> translations)
        {
            _translations = translations;
        }

        public string? GetTranslation(string name)
        {
            if (!_translations.ContainsKey(name))
            {
                return null;
            }

            return _translations[name];
        }
    }
}