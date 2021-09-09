using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;

namespace Christofel.DatabaseMigrator
{
    public class ObtainMessageChannel
    {
        private readonly MigrationChannelOptions _options;
        private readonly IDiscordRestChannelAPI _channelApi;
        
        public ObtainMessageChannel(IOptions<MigrationChannelOptions> options, IDiscordRestChannelAPI channelApi)
        {
            _channelApi = channelApi;
            _options = options.Value;
        }
        
        public async Task<Snowflake?> GetChannelAsync(ulong messageId)
        {
            var messageIdSnowflake = new Snowflake(messageId);
            foreach (var channelId in _options.ChannelId.Select(x => new Snowflake(x)))
            {
                var messageResult = await _channelApi.GetChannelMessageAsync(channelId, messageIdSnowflake);

                if (messageResult.IsSuccess)
                {
                    return channelId;
                }
            }

            return null;
        }
    }
}