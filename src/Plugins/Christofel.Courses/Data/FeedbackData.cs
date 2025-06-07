//
//  FeedbackData.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;

namespace Christofel.Courses.Data;

/// <summary>
/// Data for a feedback service,
/// with the context.
/// </summary>
/// <param name="InteractionContext">The current context.</param>
/// <param name="InteractionApi">The Discord rest interaction api instance.</param>
/// <param name="FeedbackService">The feedback service instance.</param>
public record FeedbackData
(
    IInteractionContext InteractionContext,
    IDiscordRestInteractionAPI InteractionApi,
    FeedbackService FeedbackService
);
