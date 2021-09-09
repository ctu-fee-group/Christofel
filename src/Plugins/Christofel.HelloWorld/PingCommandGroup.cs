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
    public class PingCommandGroup : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly ILogger<PingCommandGroup> _logger;

        public PingCommandGroup
        (
            ILogger<PingCommandGroup> logger,
            FeedbackService feedbackService
        )
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        [Command("ping")]
        [RequirePermission("helloworld.ping")]
        [Description("The bot will respond pong if everything went okay")]
        public Task<Result<IReadOnlyList<IMessage>>> HandlePing() => _feedbackService.SendContextualSuccessAsync
            ("Pong!");
    }
}