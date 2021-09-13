//
//   HelloWorldJob.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Scheduler.Example.Jobs
{
    /// <summary>
    /// The job that prints hello world into console.
    /// </summary>
    public class HelloWorldJob : IJob
    {
        private readonly string _where;
        private readonly ILogger<HelloWorldJob> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelloWorldJob"/> class.
        /// </summary>
        /// <param name="where">The string to be logged inside.</param>
        /// <param name="logger">The logger.</param>
        public HelloWorldJob(string where, ILogger<HelloWorldJob> logger)
        {
            _where = @where;
            _logger = logger;
        }

        /// <inheritdoc />
        public Task<Result> ExecuteAsync(IJobContext jobContext, CancellationToken ct = default)
        {
            _logger.LogInformation("Hello world from {Where}! It is {Now}", _where, DateTimeOffset.Now);
            return Task.FromResult(Result.FromSuccess());
        }
    }
}