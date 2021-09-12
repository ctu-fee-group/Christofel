//
//   JobKey.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Scheduler.Abstractions
{
    /// <summary>
    /// Represents key of a job.
    /// </summary>
    /// <param name="Group">Gets the name of the group the job is in.</param>
    /// <param name="Name">Gets the name of the command.</param>
    public record JobKey(string Group, string Name);
}