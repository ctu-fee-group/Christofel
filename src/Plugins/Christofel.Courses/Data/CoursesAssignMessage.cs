//
//  CoursesAssignMessage.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Rest.Core;

namespace Christofel.Courses.Data;

public record CoursesAssignMessage
(
    Snowflake ChannelId,
    Snowflake MessageId,
    string Prepend,
    string[] Courses,
    string Language
);