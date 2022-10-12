//
//   FeedbackServiceExtensions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Courses.Data;
using Christofel.Courses.Interactivity;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Courses.Extensions;

/// <summary>
/// Extension methods for <see cref="FeedbackService"/>.
/// </summary>
public static class FeedbackServiceExtensions
{
    /// <summary>
    /// Sends the given messages contextually.
    /// </summary>
    /// <param name="feedbackService">The feedback service.</param>
    /// <param name="messages">The messages to send.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>The messages sent or an error.</returns>
    public static async Task<Result<IReadOnlyList<IMessage>>> SendContextualMessageDataAsync
    (
        this FeedbackService feedbackService,
        IReadOnlyList<MessageData> messages,
        CancellationToken ct = default
    )
        => await new FeedbackData(null!, null!, feedbackService)
            .SendContextualMessageDataAsync(messages, false, ct);

    /// <summary>
    /// Sends the given messages contextually.
    /// </summary>
    /// <param name="feedbackData">The feedback service.</param>
    /// <param name="messages">The messages to send.</param>
    /// <param name="editOriginalResponse">Whether the first message should be sent as an edit of the original response.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>The messages sent or an error.</returns>
    public static async Task<Result<IReadOnlyList<IMessage>>> SendContextualMessageDataAsync
    (
        this FeedbackData feedbackData,
        IReadOnlyList<MessageData> messages,
        bool editOriginalResponse = false,
        CancellationToken ct = default
    )
    {
        var messageResults = new List<IMessage>();

        foreach (var messageData in messages)
        {
            if (editOriginalResponse)
            {
                var editResult = await feedbackData.InteractionApi.EditOriginalInteractionResponseAsync
                (
                    feedbackData.InteractionContext.ApplicationID,
                    feedbackData.InteractionContext.Token,
                    messageData.Content,
                    components: new Optional<IReadOnlyList<IMessageComponent>?>(messageData.Components),
                    ct: ct
                );

                if (!editResult.IsDefined(out var message))
                {
                    return Result<IReadOnlyList<IMessage>>.FromError(editResult);
                }

                editOriginalResponse = false;
                messageResults.Add(message);
            }
            else
            {
                var messageResult = await feedbackData.FeedbackService.SendContextualAsync
                (
                    messageData.Content,
                    options: new FeedbackMessageOptions
                    (
                        MessageComponents: new Optional<IReadOnlyList<IMessageComponent>>(messageData.Components),
                        MessageFlags: MessageFlags.Ephemeral
                    ),
                    ct: ct
                );

                if (!messageResult.IsDefined(out var message))
                {
                    return Result<IReadOnlyList<IMessage>>.FromError(messageResult);
                }

                messageResults.Add(message);
            }
        }

        return messageResults;
    }
}