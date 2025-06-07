//
//  IStringLocalizerFactory.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Christofel.Helpers.Localization;

/// <summary>
/// Creates <see cref="IStringLocalizer"/> instances.
/// </summary>
public interface IStringLocalizerFactory
{
    /// <summary>
    /// Create string localizer from the given type.
    /// </summary>
    /// <param name="resourceSource">The type of the resource source.</param>
    /// <returns>A string localizer.</returns>
    public IStringLocalizer Create(Type resourceSource);

    /// <summary>
    /// Create string localizer from the given name.
    /// </summary>
    /// <param name="resourceSource">The name of the resource source.</param>
    /// <returns>A string localizer.</returns>
    public IStringLocalizer Create(string resourceSource);
}