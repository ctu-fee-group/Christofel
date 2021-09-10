//
//   PingCommandGroup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Christofel.HelloWorld
{
    /// <summary>
    /// Command group responding pong to /ping command.
    /// </summary>
    public class PingCommandGroup : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly ILogger<PingCommandGroup> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PingCommandGroup"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="feedbackService">The feedback service.</param>
        public PingCommandGroup
        (
            ILogger<PingCommandGroup> logger,
            FeedbackService feedbackService
        )
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        /// <summary>
        /// Handles /ping command.
        /// </summary>
        /// <remarks>
        /// Responds with Pong.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        [Command("ping")]
        [RequirePermission("helloworld.ping")]
        [Description("The bot will respond pong if everything went okay")]
        public Task<Result<IReadOnlyList<IMessage>>> HandlePing() => _feedbackService.SendContextualSuccessAsync
            ("Pong!");
    }
}