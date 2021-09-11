//
//   RecoverableJobListener.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Scheduler.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata;
using Remora.Results;

namespace Christofel.Scheduler.Recoverable
{
    /// <summary>
    /// Removes recoverable job after.
    /// </summary>
    /// <typeparam name="TJob">The type of the job.</typeparam>
    public class RecoverableJobListener<TJob> : IJobListener
        where TJob : IJob
    {
        private readonly IJobRecoverService<TJob> _recoverableService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoverableJobListener{TJob}"/> class.
        /// </summary>
        /// <param name="recoverableService">The recoverable service.</param>
        public RecoverableJobListener(IJobRecoverService<TJob> recoverableService)
        {
            _recoverableService = recoverableService;
        }

        /// <inheritdoc />
        public ValueTask<Result> BeforeExecutionAsync
            (IJobContext context, CancellationToken ct = default) => ValueTask.FromResult(Result.FromSuccess());

        /// <inheritdoc />
        public async ValueTask<Result> AfterExecutionAsync
            (IJobContext context, Result jobResult, CancellationToken ct = default)
        {
            if (context.Job is TJob tjob)
            {
                var result = await _recoverableService.RemoveJobDataAsync(tjob, ct);
                return result;
            }

            return Result.FromSuccess();
        }
    }
}