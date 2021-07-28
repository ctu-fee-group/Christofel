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
        public void QuitBot();
    }
}