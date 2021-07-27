using System.Threading.Tasks;
using Discord.WebSocket;

namespace Christofel.BaseLib.Discord
{
    public interface IBot
    {
        public DiscordSocketClient Client { get; }

        public void QuitBot();
    }
}