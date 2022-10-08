//
//  JsonStringLocalizerFactory.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Helpers.Localization.Json;

/// <summary>
/// Constructs localization from json.
/// </summary>
public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly JsonResourceManager _resourceManager;
    private readonly ILogger<JsonStringLocalizerFactory> _logger;
    private readonly LocalizationOptions _options;
    private readonly Dictionary<string, IStringLocalizer> _cachedLocalizers;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonStringLocalizerFactory"/> class.
    /// </summary>
    /// <param name="resourceManager">The resource manager.</param>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    public JsonStringLocalizerFactory
    (
        IOptions<LocalizationOptions> options,
        ILogger<JsonStringLocalizerFactory> logger
    )
    {
        _resourceManager = new JsonResourceManager(options.Value, logger);
        _logger = logger;
        _cachedLocalizers = new Dictionary<string, IStringLocalizer>();
        _options = options.Value;
    }

    /// <inheritdoc />
    public IStringLocalizer Create(Type resourceSource)
        => Create(resourceSource.FullName ?? resourceSource.Name);

    /// <inheritdoc />
    public IStringLocalizer Create(string resourceSource)
    {
        if (_cachedLocalizers.ContainsKey(resourceSource))
        {
            return _cachedLocalizers[resourceSource];
        }

        var localizer = new JsonStringLocalizer(_resourceManager, resourceSource);
        _cachedLocalizers[resourceSource] = localizer;

        return localizer;
    }
}