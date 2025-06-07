//
//  InteractionCallbackTypeAttribute.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.CommandsLib.Attributes;

/// <summary>
/// Attribute to specify what interaction callback type to send as initial response.
/// </summary>
public class InteractionCallbackTypeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the interaction callback type.
    /// </summary>
    public InteractionCallbackType InteractionCallbackType { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionCallbackTypeAttribute"/> class.
    /// </summary>
    /// <param name="interactionCallbackType">The interaction callback type.</param>
    public InteractionCallbackTypeAttribute(InteractionCallbackType interactionCallbackType)
    {
        InteractionCallbackType = interactionCallbackType;
    }
}