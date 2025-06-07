//
//  ChannelNotCreatedError.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.Welcome;

/// <summary>
/// Cannot create a Discord channel.
/// </summary>
/// <param name="Message">The actual error description.</param>
public record ChannelNotCreatedError(string Message) : ResultError(Message);
