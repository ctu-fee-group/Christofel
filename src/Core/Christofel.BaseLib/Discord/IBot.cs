using System.Net.Http;
using Remora.Discord.Gateway;

namespace Christofel.BaseLib.Discord
{
    public interface IBot
    {
        public DiscordGatewayClient Client { get; }
        
        //public CacheService Cache { get; }
        
        public IHttpClientFactory HttpClientFactory { get; }
    }
}