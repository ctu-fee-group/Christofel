//
//   AutoThreadsOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Enhancements.AutoThreads;

/// <summary>
/// Options for <see cref="AutoThreadsResponder"/>.
/// </summary>
public class AutoThreadsOptions
{
    /// <summary>
    /// Gets or sets the channels.
    /// </summary>
    public ulong[] Channels { get; set; } = { };

    /// <summary>
    /// Gets or sets the default name for the created thread.
    /// </summary>
    public string DefaultName { get; set; } = "Automatically created thread";

    /// <summary>
    /// Gets or sets the maximal thread name length.
    /// </summary>
    public short MaxNameLength { get; set; } = 40;
}