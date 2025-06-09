//
//   AssignRoleRetryProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Jobs;
using Christofel.Scheduling;
using Christofel.Scheduling.Extensions;
using Christofel.Scheduling.Recoverable;
using Christofel.Scheduling.Retryable;
using Christofel.Scheduling.Triggers;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace Christofel.Api.Services
{
    /// <summary>
    /// Provider for retrying <see cref="CtuAuthAssignRoleJob"/>.
    /// </summary>
    public class AssignRoleRetryProvider : IRetryProvider
    {
        private const string RetryCountDataName = "RetryCount";

        private readonly IJobRecoverService<CtuAuthAssignRoleJob> _recoverService;
        private readonly IScheduler _scheduler;
        private readonly NonConcurrentTrigger.State _ncState;
        private readonly ILogger<AssignRoleRetryProvider> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignRoleRetryProvider"/> class.
        /// </summary>
        /// <param name="recoverService">The recover service.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="ncState">The state for the non concurrent trigger.</param>
        /// <param name="logger">The logger.</param>
        public AssignRoleRetryProvider
        (
            IJobRecoverService<CtuAuthAssignRoleJob> recoverService,
            IScheduler scheduler,
            NonConcurrentTrigger.State ncState,
            ILogger<AssignRoleRetryProvider> logger
        )
        {
            _recoverService = recoverService;
            _scheduler = scheduler;
            _ncState = ncState;
            _logger = logger;
        }

        /// <inheritdoc />
        public int MaxRepeatCount => 5;

        /// <inheritdoc />
        public async Task<Result> ScheduleRepeatAsync
            (IJobContext jobContext, Result jobResult, CancellationToken ct = default)
        {
            var data = jobContext.JobDescriptor.JobData;
            var name = data.Key.Name;
            var retryCount = MaxRepeatCount;
            if (data.Data.ContainsKey(RetryCountDataName))
            {
                retryCount = (int)data.Data[RetryCountDataName];
            }
            else
            {
                name += " R1";
            }

            retryCount--;
            if (retryCount == 0)
            {
                _logger.LogWarning("Used all retry attempts on this job");
                return Result.FromSuccess();
            }

            var newJobData = new TypedJobData<CtuAuthAssignRoleJob>
                    (new JobKey(data.Key.Group, name.Substring(0, name.Length - 1) + retryCount))
                .AddData("Data", data.Data)
                .AddData(RetryCountDataName, retryCount);

            var scheduleResult = await _scheduler.ScheduleAsync
                (newJobData, new NonConcurrentTrigger(new SimpleTrigger(), _ncState), ct);
            if (!scheduleResult.IsSuccess)
            {
                _logger.LogResult(scheduleResult, "Could not reschedule the job");
            }

            return Result.FromSuccess();
        }
    }
}