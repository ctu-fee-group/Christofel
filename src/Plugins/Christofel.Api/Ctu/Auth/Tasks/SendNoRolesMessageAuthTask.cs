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
using Kos;
using Kos.Abstractions;
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
        private readonly IKosAtomApi _kosAtomApi;
        private readonly IJobQueue<CtuAuthWarnMessage> _jobQueue;
        private readonly WarnOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNoRolesMessageAuthTask"/> class.
        /// </summary>
        /// <param name="kosPeopleApi">The kos people api.</param>
        /// <param name="kosAtomApi">The kos atom api.</param>
        /// <param name="jobQueue">The job queue.</param>
        /// <param name="options">The options.</param>
        public SendNoRolesMessageAuthTask
        (
            IKosPeopleApi kosPeopleApi,
            IKosAtomApi kosAtomApi,
            IJobQueue<CtuAuthWarnMessage> jobQueue,
            IOptionsSnapshot<WarnOptions> options
        )
        {
            _options = options.Value;
            _kosAtomApi = kosAtomApi;
            _jobQueue = jobQueue;
            _kosPeopleApi = kosPeopleApi;
        }

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
        {
            var kosPerson = await _kosPeopleApi.GetPersonAsync(data.LoadedUser.CtuUsername, ct);
            var kosStudent = await _kosAtomApi.LoadEntityAsync(kosPerson?.Roles.Students.LastOrDefault(), ct);
            if (data.Roles.AddRoles.Count == 1 &&
                (kosStudent is null || kosStudent.StartDate > DateTime.Now.Subtract(TimeSpan.FromDays(5))))
            {
                _jobQueue.EnqueueJob(new CtuAuthWarnMessage(data.LoadedUser.DiscordId, _options.NoRolesMessage));
            }

            return Result.FromSuccess();
        }
    }
}