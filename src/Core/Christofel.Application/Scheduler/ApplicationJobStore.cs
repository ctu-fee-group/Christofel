//
//   ApplicationJobStore.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib;
using Christofel.BaseLib.Plugins;
using Christofel.Plugins;
using Christofel.Scheduler.Abstractions;
using Remora.Results;

namespace Christofel.Application.Scheduler
{
    /// <summary>
    /// Job store returning values from plugin storages.
    /// </summary>
    public class ApplicationJobStore : IJobStore
    {
        private readonly PluginStorage _pluginStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationJobStore"/> class.
        /// </summary>
        /// <param name="pluginStorage">The storage of the plugins.</param>
        public ApplicationJobStore(PluginStorage pluginStorage)
        {
            _pluginStorage = pluginStorage;
        }

        /// <inheritdoc />
        public ValueTask<Result<IJobDescriptor>> AddJobAsync
            (IJob job, ITrigger trigger)
            => ValueTask.FromResult<Result<IJobDescriptor>>(new NotSupportedError());

        /// <inheritdoc />
        public async ValueTask<Result> RemoveJobAsync(string jobKey)
        {
            var errors = new List<IResult>();

            foreach (var plugin in _pluginStorage.AttachedPlugins
                .Select(x => x.Plugin)
                .OfType<IRuntimePlugin<IChristofelState, PluginContext>>())
            {
                if (plugin.Context.SchedulerJobStore is not null)
                {
                    var removalResult = await plugin.Context.SchedulerJobStore.RemoveJobAsync(jobKey);
                    if (!removalResult.IsSuccess)
                    {
                        errors.Add(removalResult);
                    }
                }
            }

            return errors.Count > 0
                ? new AggregateError(errors)
                : Result.FromSuccess();
        }

        /// <inheritdoc />
        public IEnumerable<IJobDescriptor> EnumerateJobs()
        {
            foreach (var plugin in _pluginStorage.AttachedPlugins
                .Select(x => x.Plugin)
                .OfType<IRuntimePlugin<IChristofelState, PluginContext>>())
            {
                if (plugin.Context.SchedulerJobStore is not null)
                {
                    foreach (var storedJob in plugin.Context.SchedulerJobStore.EnumerateJobs())
                    {
                        yield return storedJob;
                    }
                }
            }
        }
    }
}