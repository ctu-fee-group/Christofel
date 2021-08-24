using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Application.Extensions;
using Christofel.BaseLib.Extensions;

namespace Christofel.Application.Logging.Discord
{
    public class DiscordLoggerProcessor : IDisposable
    {
        // TODO: how to handle exceptions here?

        private readonly BlockingCollection<DiscordLogMessage> _messageQueue;
        private readonly Thread _outputThread;

        private readonly DiscordSocketClient _bot;

        private bool _canProcess;
        private bool _quit;

        public DiscordLoggerProcessor(DiscordSocketClient bot, DiscordLoggerOptions options)
        {
            _canProcess = false;
            _bot = bot;
            _bot.Ready += HandleBotReady;

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
            if (_bot.ConnectionState != ConnectionState.Connected)
            {
                _canProcess = false;
                return false;
            }

            IMessageChannel channel = _bot
                .GetGuild(entry.GuildId)
                .GetTextChannel(entry.ChannelId);

            if (channel != null)
            {
                foreach (string part in entry.Message.Chunk(2000))
                {
                    channel.SendMessageAsync(part, allowedMentions: AllowedMentions.None).GetAwaiter().GetResult();
                }
            }
            else
            {
                Console.WriteLine("Could not find log channel");
            }

            return true;
        }

        private void SendMessages(List<DiscordLogMessage> messages)
        {
            foreach (DiscordLogMessage message in messages)
            {
                bool sent = false;

                while (!sent)
                {
                    while (!_canProcess)
                    {
                        Thread.Sleep(10);
                    }

                    try
                    {
                        sent = SendMessage(message);
                    }
                    catch (Exception) // Generally exceptions shouldn't happen here
                    {
                        sent = true; // pretend like it was sent
                        //EnqueueMessage(message); // add it to queue so we don't lose it
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

        protected Task HandleBotReady()
        {
            _canProcess = true;
            return Task.CompletedTask;
        }
    }
}