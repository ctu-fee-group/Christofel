//
//  ICultureProvider.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Helpers.Localization;

/// <summary>
/// Provides current culture.
/// </summary>
public interface ICultureProvider
{
    /// <summary>
    /// Gets the current culture.
    /// </summary>
    public string CurrentCulture { get; }
}