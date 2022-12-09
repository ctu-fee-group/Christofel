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
using Christofel.Api.Ctu.JobQueue;
using Christofel.Helpers.JobQueue;
using Kos;
using Kos.Abstractions;
using Kos.Data;
using Microsoft.Extensions.Options;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Tasks
{
    /// <summary>
    /// Sends direct message to the user if he had no roles assigned.
    /// </summary>
    public class SendNoRolesMessageAuthTask : IAuthTask
    {
        private readonly IKosPeopleApi _kosPeopleApi;
        private readonly IKosStudentsApi _kosStudentsApi;
        private readonly IJobQueue<CtuAuthWarnMessage> _jobQueue;
        private readonly WarnOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNoRolesMessageAuthTask"/> class.
        /// </summary>
        /// <param name="kosPeopleApi">The kos people api.</param>
        /// <param name="kosStudentsApi">The kos students api.</param>
        /// <param name="jobQueue">The job queue.</param>
        /// <param name="options">The options.</param>
        public SendNoRolesMessageAuthTask
        (
            IKosPeopleApi kosPeopleApi,
            IKosStudentsApi kosStudentsApi,
            IJobQueue<CtuAuthWarnMessage> jobQueue,
            IOptionsSnapshot<WarnOptions> options
        )
        {
            _options = options.Value;
            _jobQueue = jobQueue;
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
                _jobQueue.EnqueueJob(new CtuAuthWarnMessage(data.LoadedUser.DiscordId, _options.NoRolesMessage));
            }

            return Result.FromSuccess();
        }
    }
}