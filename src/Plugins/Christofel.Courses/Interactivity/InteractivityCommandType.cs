//
//   InteractivityCommandType.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Courses.Interactivity;

/// <summary>
/// A type of operation to initiate on the selected course.
/// </summary>
public enum InteractivityCommandType
{
    /// <summary>
    /// Join the course channel.
    /// </summary>
    Join,

    /// <summary>
    /// Leave the course channel.
    /// </summary>
    Leave,

    /// <summary>
    /// Toggle the course channel.
    /// </summary>
    Toggle
}