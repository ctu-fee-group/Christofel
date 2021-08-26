using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Christofel.Logger
{
    public class DiscordLoggerProcessor : IDisposable
    {
        // TODO: how to handle exceptions here?

        private readonly BlockingCollection<DiscordLogMessage> _messageQueue;
        private readonly Thread _outputThread;
        
        private bool _disposed;
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
            if (!_disposed)
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
        
        private static IEnumerable<string> Chunk(string? str, int chunkSize) =>
            !string.IsNullOrEmpty(str) ?
                Enumerable.Range(0, (int)Math.Ceiling(((double)str.Length) / chunkSize))
                    .Select(i => str
                        .Substring(i * chunkSize,
                            (i * chunkSize + chunkSize <= str.Length) ? chunkSize : str.Length - i * chunkSize))
                : Enumerable.Empty<string>();

        private bool SendMessage(DiscordLogMessage entry)
        {
            if (_disposed)
            {
                return false;
            }
            
            if (_channelApi is null)
            {
                _channelApi = _provider.GetRequiredService<IDiscordRestChannelAPI>();
            }

            bool success = true;
            Optional<IMessageReference> message = default;
            foreach (string part in Chunk(entry.Message, 2000))
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
            int retries = 5;
            foreach (DiscordLogMessage message in messages)
            {
                bool sent = false;

                while (!sent && retries-- > 0)
                {
                    try
                    {
                        sent = SendMessage(message);
                    }
                    catch (Exception) // Generally exceptions shouldn't happen here
                    {
                        sent = false;
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
                _outputThread.Join(20000);
                ProcessLogQueue();
                _disposed = true;
            }
            catch (ThreadStateException)
            {
            }
        }
    }
}