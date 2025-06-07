//
//   AtomLoadableExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Kos.Atom;

namespace Christofel.CoursesLib.Extensions;

/// <summary>
/// Extension methods for <see cref="AtomLoadableEntity"/>.
/// </summary>
public static class AtomLoadableExtensions
{
    /// <summary>
    /// Gets the key part of the loadable entity.
    /// </summary>
    /// <param name="loadableEntity">The loadable entity to get key from.</param>
    /// <typeparam name="T">The type of loadable entity.</typeparam>
    /// <returns>The key part of the loadable entity.</returns>
    public static string GetKey<T>(this AtomLoadableEntity<T> loadableEntity)
        where T : new()
        => loadableEntity.Href.Split('/', StringSplitOptions.RemoveEmptyEntries)[^1];
}