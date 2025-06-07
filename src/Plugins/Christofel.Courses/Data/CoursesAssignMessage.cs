//
//  CoursesAssignMessage.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Rest.Core;

namespace Christofel.Courses.Data;

/// <summary>
/// Information about courses for given assignment
/// request from the user.
/// </summary>
/// <param name="ChannelId">Id of the channel.</param>
/// <param name="MessageId">Id of the message.</param>
/// <param name="Prepend">What to prepend before the message.</param>
/// <param name="Courses">What courses were matched.</param>
/// <param name="Language">What language to show the message in.</param>
public record CoursesAssignMessage
(
    Snowflake ChannelId,
    Snowflake MessageId,
    string Prepend,
    string[] Courses,
    string Language
);
