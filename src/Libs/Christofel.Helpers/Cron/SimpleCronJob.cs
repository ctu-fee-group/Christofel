//
//   SimpleCronJob.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Helpers.Cron;

/// <summary>
/// Just a simple cron job using Task.
/// </summary>
public abstract class SimpleCronJob : ICronJob
{
    // TODO: make thread-safe
    //   currently there is a race condition here.
    //   this is not so important for now since it is not expected
    //   the task will be rescheduled at all and if it will be,
    //   it will be rescheduled very occasionally.

    private readonly ILogger _logger;
    private readonly string _name;
    private CancellationTokenSource? _ctsource;
    private CancellationTokenSource? _stopSource;
    private Task? _runTask;
    private bool _run;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleCronJob"/> class.
    /// </summary>
    /// <param name="name">Name of the cron job used in log messages.</param>
    /// <param name="logger">The logger to log errors and info messages with.</param>
    /// <param name="interval">Interval between job schedules.</param>
    /// <param name="firstOffset">The first offset from initialization to run the job at. Default is immediately.</param>
    protected SimpleCronJob
        (
            string name,
            ILogger logger,
            TimeSpan interval,
            TimeSpan firstOffset = default
        )
    {
        _logger = logger;
        _name = name;

        NextScheduledTime = DateTime.Now + firstOffset;
        Interval = interval;
    }

    /// <summary>
    /// Do the job.
    /// </summary>
    /// <param name="manual">Whether this has been called manually (true) or on scheduled time (false).</param>
    /// <returns>A task representing the job.</returns>
    protected abstract Task<IResult> ProcessAsync(bool manual);

    /// <inheritdoc/>
    public DateTime NextScheduledTime { get; private set; }

    /// <inheritdoc/>
    public TimeSpan Interval { get; private set; }

    /// <inheritdoc/>
    public Task<IResult> RescheduleNextAsync(DateTime date)
    {
        NextScheduledTime = date;
        _ctsource?.Cancel();
        return Task.FromResult((IResult)Result.FromSuccess());
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken token = default)
    {
        if (_run)
        {
            return Task.CompletedTask;
        }

        _run = true;
        _ctsource = new CancellationTokenSource();
        _stopSource = new CancellationTokenSource();
        _runTask = Task.Run(async () => await RunTask());
        return Task.CompletedTask;
    }

    private async Task RunTask()
    {
        while (_run)
        {
            try
            {
                var nextScheduledDelay = NextScheduledTime - DateTime.Now;
                if (nextScheduledDelay <= TimeSpan.FromSeconds(1))
                {
                    var result = await ProcessAsync(false);

                    if (!result.IsSuccess)
                    {
                        _logger.LogResultError
                            (
                                result,
                                "An error has been encountered when running cron task job."
                            );
                    }
                    else
                    {
                        _logger.LogInformation("Successfully ran job.");
                    }

                    NextScheduledTime = DateTime.Now + Interval;
                    nextScheduledDelay = Interval;
                }

                try
                {
                    if (_ctsource == null)
                    {
                        _ctsource = new CancellationTokenSource();
                    }

                    await Task.Delay(nextScheduledDelay, _ctsource.Token);
                }
                catch (TaskCanceledException)
                {
                    _ctsource = new CancellationTokenSource();
                }
            }
            catch (Exception ex)
            {
                NextScheduledTime = DateTime.Now + Interval;
                _logger.LogError(ex, "An exception has been thrown in a cron job task. Skipping execution.");
            }
        }

        _stopSource?.Cancel();
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken token = default)
    {
        if (!_run)
        {
            return;
        }

        _run = false;
        _ctsource?.Cancel();

        if (_stopSource is null)
        {
            return;
        }

        try
        {
            await Task.Delay(-1, _stopSource.Token);
        }
        catch
        {
            // Do nothing.
        }

        _runTask?.Dispose();

        _runTask = null;
        _ctsource = null;
        _stopSource = null;
    }

    /// <inheritdoc/>
    public Task<IResult> TriggerNowAsync()
    {
        return ProcessAsync(true);
    }
}
