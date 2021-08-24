using System.Net.Http;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway;

namespace Christofel.BaseLib.Discord
{
    /// <summary>
    /// State of the discord bot
    /// </summary>
    public interface IBot
    {
        public DiscordGatewayClient Client { get; }
        
        public CacheService Cache { get; }
        
        public IHttpClientFactory HttpClientFactory { get; }
    }
}