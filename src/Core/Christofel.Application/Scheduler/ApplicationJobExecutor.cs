//
//  ApplicationJobExecutor.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Plugins;
using Christofel.Common;
using Christofel.Plugins;
using Christofel.Scheduling;
using Remora.Results;

namespace Christofel.Application.Scheduler
{
    /// <summary>
    /// Executor of jobs that distributes the job to the plugins.
    /// </summary>
    public class ApplicationJobExecutor : IJobExecutor
    {
        private readonly PluginStorage _pluginStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationJobExecutor"/> class.
        /// </summary>
        /// <param name="pluginStorage">The storage of the plugins.</param>
        public ApplicationJobExecutor(PluginStorage pluginStorage)
        {
            _pluginStorage = pluginStorage;
        }

        /// <inheritdoc />
        public async Task<Result<IJobContext>> BeginExecutionAsync
        (
            IJobDescriptor jobDescriptor,
            Func<IJobDescriptor, Task> afterExecutionCallback,
            CancellationToken ct = default
        )
        {
            var errors = new List<IResult>();
            foreach (var plugin in _pluginStorage.AttachedPlugins
                .Select(x => x.Plugin)
                .OfType<IRuntimePlugin<IChristofelState, PluginContext>>())
            {
                if (plugin.Context.SchedulerJobExecutor is not null)
                {
                    var executionResult = await plugin.Context.SchedulerJobExecutor.BeginExecutionAsync
                        (jobDescriptor, afterExecutionCallback, ct);
                    if (executionResult.IsSuccess)
                    {
                        return executionResult;
                    }

                    errors.Add(executionResult);
                }
            }

            return errors.Count > 0
                    ? new AggregateError(errors)
                    : new InvalidOperationError("Could not find any executor inside of plugins.");
        }
    }
}