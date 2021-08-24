using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Lifetime;
using Christofel.CommandsLib;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;

namespace Christofel.Application.Commands
{
    /// <summary>
    /// Handler of /refresh and /quit commands
    /// </summary>
    [DiscordDefaultPermission(false)]
    public class ControlCommands : CommandGroup
    {
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly RefreshChristofel _refresh;
        private readonly ILogger<ControlCommands> _logger;
        private readonly FeedbackService _feedbackService;

        public ControlCommands(
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
        private async Task HandleRefreshCommand()
        {
            _logger.LogInformation("Handling command /refresh");
            await _refresh(CancellationToken);
            _logger.LogInformation("Refreshed successfully");
            
            await _feedbackService.SendContextualSuccessAsync("Successfully refreshed", ct: CancellationToken);
        }

        [Command("quit")]
        [Description("Exit the application")]
        [RequirePermission("application.quit")]
        private Task HandleQuitCommand()
        {
            _logger.LogInformation("Handling command /quit");
            _applicationLifetime.RequestStop();

            return _feedbackService.SendContextualSuccessAsync("Goodbye", ct: CancellationToken);
        }
    }
}