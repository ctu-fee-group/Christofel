//
//  JsonResource.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Helpers.Localization.Json;

/// <summary>
/// Resource for a localizer.
/// </summary>
/// <param name="Name">Identifier of the resource.</param>
/// <param name="Value">Value of the resource.</param>
public record JsonResource(string Name, string Value);
