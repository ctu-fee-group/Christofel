//
//   MessageData.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CoursesLib.Database;
using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.Courses.Data;

/// <summary>
/// Data of a message to be sent.
/// </summary>
/// <param name="Content">The content of the message.</param>
/// <param name="Components">The message components.</param>
/// <param name="Courses">The courses in this message, if any.</param>
public record MessageData
(
    string Content,
    IReadOnlyList<IMessageComponent> Components,
    IReadOnlyList<CourseAssignment>? Courses = default
);