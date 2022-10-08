//
//   MessageData.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.Courses.Interactivity;

/// <summary>
/// Data of a message to be sent.
/// </summary>
/// <param name="Content">The content of the message.</param>
/// <param name="Components">The message components.</param>
public record MessageData
(
    string Content,
    IReadOnlyList<IMessageComponent> Components
);