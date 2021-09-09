//
//   ControlCommandGroup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Christofel.Application.Commands
{
    /// <summary>
    ///     Handler of /refresh and /quit commands
    /// </summary>
    public class ControlCommands : CommandGroup
    {
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly FeedbackService _feedbackService;
        private readonly ILogger<ControlCommands> _logger;
        private readonly RefreshChristofel _refresh;

        public ControlCommands
        (
            FeedbackService feedbackService,
            IApplicationLifetime lifetime,
            RefreshChristofel refresh,
            ILogger<ControlCommands> logger
        )
        {
            _feedbackService = feedbackService;
            _logger = logger;
            _applicationLifetime = lifetime;
            _refresh = refresh;
        }

        [Command("refresh")]
        [Description("Refresh the application and all plugins, reloading permissions, configuration and such")]
        [RequirePermission("application.refresh")]
        [Ephemeral]
        public async Task<IResult> HandleRefreshCommand()
        {
            await _refresh(CancellationToken);
            _logger.LogInformation("Refreshed successfully");

            return await _feedbackService.SendContextualSuccessAsync("Successfully refreshed", ct: CancellationToken);
        }

        [Command("quit")]
        [Description("Exit the application")]
        [RequirePermission("application.quit")]
        [Ephemeral]
        public Task<Result<IReadOnlyList<IMessage>>> HandleQuitCommand()
        {
            _applicationLifetime.RequestStop();

            return _feedbackService.SendContextualSuccessAsync("Goodbye", ct: CancellationToken);
        }
    }
}