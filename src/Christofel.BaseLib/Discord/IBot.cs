using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Christofel.BaseLib.Discord
{
    /// <summary>
    /// State of the discord bot
    /// </summary>
    public interface IBot
    {
        /// <summary>
        /// The socket client itself to allow Discord API usage and events handling
        /// </summary>
        public DiscordSocketClient Client { get; }

        /// <summary>
        /// Method to tell the application to quit
        /// </summary>
        /// <remarks>
        /// In the end does the same thing as calling RequestStop on IApplicationLifetime
        /// </remarks>
        [Obsolete]
        public void QuitBot();
    }
}