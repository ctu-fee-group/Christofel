//
//   SendNoRolesMessageAuthTask.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Auth.Tasks.Options;
using Christofel.Api.Ctu.Jobs;
using Christofel.Scheduler;
using Christofel.Scheduler.Abstractions;
using Christofel.Scheduler.Triggers;
using Kos;
using Kos.Abstractions;
using Kos.Data;
using Microsoft.Extensions.Options;
using Remora.Results;

namespace Christofel.CtuAuth.Auth.Tasks
{
    /// <summary>
    /// Sends direct message to the user if he had no roles assigned.
    /// </summary>
    public class SendNoRolesMessageAuthTask : IAuthTask
    {
        private readonly NonConcurrentTrigger.State _ncState;
        private readonly IKosPeopleApi _kosPeopleApi;
        private readonly IKosStudentsApi _kosStudentsApi;
        private readonly IScheduler _scheduler;
        private readonly WarnOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNoRolesMessageAuthTask"/> class.
        /// </summary>
        /// <param name="kosPeopleApi">The kos people api.</param>
        /// <param name="kosStudentsApi">The kos students api.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="options">The options.</param>
        /// <param name="ncState">The state of the non concurrency.</param>
        public SendNoRolesMessageAuthTask
        (
            IKosPeopleApi kosPeopleApi,
            IKosStudentsApi kosStudentsApi,
            IOptionsSnapshot<WarnOptions> options,
            IScheduler scheduler
        )
        {
            _options = options.Value;
            _options = options.Value;
            _scheduler = scheduler;
            _ncState = ncState;
            _kosPeopleApi = kosPeopleApi;
            _kosStudentsApi = kosStudentsApi;
        }

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
        {
            var kosPerson = await _kosPeopleApi.GetPersonAsync(data.LoadedUser.CtuUsername, ct);
            var studentLoadable = kosPerson?.Roles.Students.LastOrDefault();
            Student? kosStudent = null;

            if (studentLoadable is not null)
            {
                kosStudent = await _kosStudentsApi.GetStudent(studentLoadable, token: ct);
            }

            if (data.Roles.AddRoles.Count == 1 &&
                (kosStudent is null || kosStudent.StartDate > DateTime.Now.Subtract(TimeSpan.FromDays(5))))
            {
                var jobData = new TypedJobData<CtuAuthWarnMessageJob>
                    (
                        JobKeyUtils.GenerateRandom
                            ("Auth", $"Send warn message to <{data.LoadedUser.DiscordId.ToString()}> ")
                    )
                    .AddData("Data", new CtuAuthWarnMessage(data.LoadedUser.DiscordId, _options.NoRolesMessage));
                var scheduleResult = await _scheduler.ScheduleAsync
                    (jobData, new NonConcurrentTrigger(new SimpleTrigger(), _ncState));
                if (!scheduleResult.IsSuccess)
                {
                    return Result.FromError(scheduleResult);
                }
            }

            return Result.FromSuccess();
        }
    }
}
