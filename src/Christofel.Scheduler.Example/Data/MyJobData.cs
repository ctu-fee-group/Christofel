//
//  MyJobData.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Scheduler.Example.Jobs;

namespace Christofel.Scheduler.Example.Data
{
    /// <summary>
    /// The data for <see cref="PassDataJob"/>.
    /// </summary>
    /// <param name="PrintString">The string to print.</param>
    /// <param name="PrintNumber">The number to print.</param>
    public record MyJobData(string PrintString, int PrintNumber);
}