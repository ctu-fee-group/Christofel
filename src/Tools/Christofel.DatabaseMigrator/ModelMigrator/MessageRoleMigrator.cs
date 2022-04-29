using System.Threading.Tasks;
using Christofel.DatabaseMigrator.Model;
using Christofel.ReactHandler.Database;
using Christofel.ReactHandler.Database.Models;
using Microsoft.Extensions.Logging;
using Remora.Rest.Core;

namespace Christofel.DatabaseMigrator.ModelMigrator
{
    public class MessageRoleMigrator : IModelMigrator
    {
        private readonly ReactHandlerContext _reactHandlerContext;
        private readonly OldContext _oldContext;
        private readonly ObtainMessageChannel _obtainMessageChannel;
        private readonly ILogger _logger;

        public MessageRoleMigrator(ReactHandlerContext reactHandlerContext, OldContext oldContext,
            ObtainMessageChannel obtainMessageChannel, ILogger<MessageRoleMigrator> logger)
        {
            _obtainMessageChannel = obtainMessageChannel;
            _oldContext = oldContext;
            _reactHandlerContext = reactHandlerContext;
            _logger = logger;
        }

        public async Task MigrateModel()
        {
            await foreach (var messageChannel in _oldContext.MessageChannels)
            {
                var channel = await _obtainMessageChannel.GetChannelAsync(ulong.Parse(messageChannel.MessageId));
                if (channel is null)
                {
                    _logger.LogWarning("Could not find channel for message {MessageId}", messageChannel.MessageId);
                    continue;
                }

                var clonedEntity = new HandleReact()
                {
                    ChannelId = channel.Value,
                    MessageId = new Snowflake(ulong.Parse(messageChannel.MessageId)),
                    Emoji = EmojiMigrator.ConvertEmoji(messageChannel.EmojiId),
                    Type = HandleReactType.Channel,
                    EntityId = new Snowflake(ulong.Parse(messageChannel.ChannelId)),
                };

                _reactHandlerContext.Add(clonedEntity);
            }

            await _reactHandlerContext.SaveChangesAsync();
        }
    }
}