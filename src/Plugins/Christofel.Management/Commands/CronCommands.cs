//
//   CronCommands.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using Christofel.Helpers.Cron;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Christofel.Management.Commands;

/// <summary>
/// Command group for cron management commands.
/// </summary>
[Group("cron")]
[Description("Manage crons")]
[RequirePermission("management.cron")]
[Ephemeral]
public class CronCommands : CommandGroup
{
    private readonly FeedbackService _feedback;
    private readonly IServiceProvider _services;
    private readonly CronRepository _crons;

    /// <summary>
    /// Initializes a new instance of the <see cref="CronCommands"/> class.
    /// </summary>
    /// <param name="feedback">The feedback service to provide feedback to user.</param>
    /// <param name="services">The service provider to initialize cron from.</param>
    /// <param name="crons">The repository with crons.</param>
    public CronCommands
        (
            FeedbackService feedback,
            IServiceProvider services,
            CronRepository crons
        )
    {
        _feedback = feedback;
        _services = services;
        _crons = crons;
    }

    /// <summary>
    /// Sends a list of available crons.
    /// </summary>
    /// <returns>A result that may not have succeeded.</returns>
    [Command("list")]
    [RequirePermission("management.cron.list")]
    public async Task<IResult> HandleList()
    {
        var cronNames = _crons.Crons;
        var crons = cronNames
            .Select(x => (x, _crons.TryGetCron(_services, x)));

        string listMessage = "List of crons:\n" +
            string.Join
            (
                "\n",
                crons.Select
                (x => $"  {x.x}, next execution: {x.Item2!.NextScheduledTime}")
            ) + "\n";

        return await _feedback.SendContextualSuccessAsync
            (
                listMessage,
                ct: CancellationToken
            );
    }

    /// <summary>
    /// Executes the given cron right now.
    /// </summary>
    /// <param name="name">The name of the cron to execute.</param>
    /// <returns>A result that may not have succeeded.</returns>
    [Command("execute")]
    [RequirePermission("management.cron.execute")]
    public async Task<IResult> HandleExecute([Description("Name of the cron to execute, now.")] string name)
    {
        var cron = _crons.TryGetCron(_services, name);

        if (cron is null)
        {
            return await _feedback.SendContextualErrorAsync
                ($"Cron with name {name} has not been found.");
        }

        var result = await cron.TriggerNowAsync();

        if (!result.IsSuccess)
        {
            await _feedback.SendContextualErrorAsync("An error has occured when executing the cron.");
            return result;
        }

        return await _feedback.SendContextualSuccessAsync($"Successfully ran {name} cron.");
    }
}
