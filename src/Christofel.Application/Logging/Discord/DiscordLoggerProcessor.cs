using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Application.Extensions;
using Christofel.BaseLib.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Christofel.Application.Logging.Discord
{
    public class DiscordLoggerProcessor : IDisposable
    {
        // TODO: how to handle exceptions here?

        private readonly BlockingCollection<DiscordLogMessage> _messageQueue;
        private readonly Thread _outputThread;
        
        private bool _quit;
        private IServiceProvider _provider;
        private IDiscordRestChannelAPI? _channelApi;

        public DiscordLoggerProcessor(IServiceProvider provider, DiscordLoggerOptions options)
        {
            _provider = provider;

            Options = options;
            _messageQueue = new BlockingCollection<DiscordLogMessage>((int)Options.MaxQueueSize);

            _outputThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "Discord logger queue processing thread"
            };
            _outputThread.Start();
        }

        public DiscordLoggerOptions Options { get; set; }

        public virtual void EnqueueMessage(DiscordLogMessage message)
        {
            if (!_quit)
            {
                try
                {
                    _messageQueue.Add(message);
                    return;
                }
                catch (InvalidOperationException)
                {
                }
            }

            // Adding is completed so just log the message
            try
            {
                SendMessage(message);
            }
            catch (Exception)
            {
            }
        }

        private bool SendMessage(DiscordLogMessage entry)
        {
            if (_channelApi is null)
            {
                _channelApi = _provider.GetRequiredService<IDiscordRestChannelAPI>();
            }
            
            bool success = true;
            Optional<IMessageReference> message = default;
            foreach (string part in entry.Message.Chunk(2000))
            {
                var result =
                    _channelApi.CreateMessageAsync(new Snowflake(entry.ChannelId), part, messageReference: message,
                        allowedMentions:
                        new AllowedMentions(Roles: new List<Snowflake>(),
                            Users: new List<Snowflake>())).GetAwaiter().GetResult();

                if (!result.IsSuccess)
                {
                    success = false;
                    Console.WriteLine("There was an error when logging: " + result.Error + result.Error.Message);
                    break;
                }

                message = new MessageReference(result.Entity.ID, result.Entity.ChannelID, result.Entity.GuildID);
            }

            return success;
        }

        private void SendMessages(List<DiscordLogMessage> messages)
        {
            foreach (DiscordLogMessage message in messages)
            {
                bool sent = false;

                while (!sent)
                {
                    try
                    {
                        sent = SendMessage(message);
                    }
                    catch (Exception) // Generally exceptions shouldn't happen here
                    {
                        sent = false; // pretend like it was sent
                    }
                }
            }
        }

        private void ProcessLogQueue()
        {
            try
            {
                // TODO: refactor to make shorter
                int count = 0;
                while (!_messageQueue.IsCompleted)
                {
                    List<DiscordLogMessage> fetchedMessages =
                        _messageQueue.FetchAtLeastOneBlocking(Options.MaxGroupSize);

                    List<DiscordLogMessage> messagesToSend = fetchedMessages
                        .GroupBy(x => new { x.GuildId, x.ChannelId })
                        .Select(x =>
                            new DiscordLogMessage(x.Key.GuildId, x.Key.ChannelId,
                                string.Join('\n', x.Select(x => x.Message)))
                        ).ToList();

                    SendMessages(messagesToSend);
                }
            }
            catch (Exception e)
            {
                try
                {
                    _messageQueue.CompleteAdding();
                }
                catch
                {
                }
            }
        }

        public void Dispose()
        {
            _messageQueue.CompleteAdding();

            try
            {
                _outputThread.Join(1500);
                ProcessLogQueue();
            }
            catch (ThreadStateException)
            {
            }
        }
    }
}