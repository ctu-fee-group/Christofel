//
//  EditInteractionResponseTask.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Auth.Tasks.Options;
using Christofel.Api.Ctu.JobQueue;
using Christofel.Helpers.JobQueue;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Results;

namespace Christofel.Api.Ctu.Auth.Tasks;

/// <summary>
/// Edits the interaction associated with the user, if possible.
/// </summary>
public class EditInteractionResponseTask : IAuthTask
{
    private readonly IJobQueue<CtuAuthInteractionEdit> _jobQueue;
    private readonly EditInteractionOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditInteractionResponseTask"/> class.
    /// </summary>
    /// <param name="jobQueue">The edit interaction operation job queue.</param>
    /// <param name="options">The options.</param>
    public EditInteractionResponseTask
    (
        IJobQueue<CtuAuthInteractionEdit> jobQueue,
        IOptionsSnapshot<EditInteractionOptions> options
    )
    {
        _jobQueue = jobQueue;
        _options = options.Value;
    }

    /// <inheritdoc />
    public Task<Result> ExecuteAsync(IAuthData data, CancellationToken ct = default)
    {
        if (data.DbUser.InteractionToken is not null)
        {
            _jobQueue.EnqueueJob(new CtuAuthInteractionEdit(data.DbUser.InteractionToken, _options.EditedMessage));
        }

        return Task.FromResult(Result.FromSuccess());
    }
}