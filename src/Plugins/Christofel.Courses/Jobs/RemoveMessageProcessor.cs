//
// RemoveMessageProcessor.cs
//
// Copyright (c) Christofel authors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Courses.Data;
using Christofel.Helpers.JobQueue;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;
using Remora.Discord.Interactivity.Services;
using Remora.Rest.Core;

namespace Christofel.Courses.Jobs;

/// <summary>
/// A queue for removing <see cref="CoursesAssignMessage"/> from memory data service.
/// </summary>
public class RemoveMessageProcessor : ThreadPoolJobQueue<RemoveMessageProcessor.RemoveMessage>
{
    /// <summary>
    /// The time to keep the information about courses message in memory service in seconds.
    /// </summary>
    private const int KEEP_MEMORY_COURSES_TIME = 20 * 60 * 1000;

    private readonly InMemoryDataService<Snowflake, CoursesAssignMessage> _memoryDataService;
    private readonly ICurrentPluginLifetime _lifetime;

    /// <inheritdoc cref="ThreadPoolJobQueue"/>
    public RemoveMessageProcessor
    (
        InMemoryDataService<Snowflake, CoursesAssignMessage> memoryDataService,
        ICurrentPluginLifetime lifetime,
        ILogger<RemoveMessageProcessor> logger
    )
        : base(lifetime, logger)
    {
        _memoryDataService = memoryDataService;
        _lifetime = lifetime;
    }

    /// <inheritdoc />
    protected override async Task ProcessAssignJob(RemoveMessage job)
    {
        var endTime = job.AddedTime.AddMilliseconds(KEEP_MEMORY_COURSES_TIME);
        if (DateTime.Now < endTime)
        {
            await Task.Delay(endTime - DateTime.Now, _lifetime.Stopping);
        }

        await _memoryDataService.TryRemoveDataAsync(job.MessageId);
    }

    /// <summary>
    /// The job for <see cref="RemoveMessageProcessor"/>.
    /// </summary>
    /// <param name="MessageId">The id of the message to remove from the memory.</param>
    public record RemoveMessage
    (
        Snowflake MessageId,
        DateTime AddedTime
    );
}