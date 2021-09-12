//
//   PluginContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Remora;

namespace Christofel.BaseLib.Plugins
{
    /// <summary>
    /// Context implementation of the plugin context.
    /// </summary>
    public class PluginContext : IPluginContext
    {
        /// <summary>
        /// Gets the job executor.
        /// </summary>
        public IJobExecutor? SchedulerJobExecutor { get; set; }

        /// <summary>
        /// Gets responder of the plugin.
        /// </summary>
        public IAnyResponder? PluginResponder { get; set; }
    }
}
