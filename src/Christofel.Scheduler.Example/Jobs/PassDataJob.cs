//
//   PassDataJob.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Christofel.Scheduler.Example.Data;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Scheduler.Example.Jobs
{
    /// <summary>
    /// Prints given data.
    /// </summary>
    public class PassDataJob : IJob
    {
        private readonly MyJobData _data;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassDataJob"/> class.
        /// </summary>
        /// <param name="data">The data to be printed.</param>
        /// <param name="logger">The logger.</param>
        public PassDataJob(MyJobData data, ILogger<PassDataJob> logger)
        {
            _data = data;
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<Result> ExecuteAsync(IJobContext jobContext, CancellationToken ct = default)
        {
            _logger.LogInformation
                ("Data received in pass data job: {PrintString} {PrintNumber}", _data.PrintString, _data.PrintNumber);
            return Task.FromResult(Result.FromSuccess());
        }
    }
}