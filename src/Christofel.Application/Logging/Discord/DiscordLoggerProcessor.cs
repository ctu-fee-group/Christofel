using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Discord;
using Christofel.BaseLib.Extensions;
using Discord;
using Discord.WebSocket;

namespace Christofel.Application.Logging.Discord
{
    public class DiscordLoggerProcessor : IDisposable
    {
        // TODO: how to log exceptions from here?

        private readonly BlockingCollection<DiscordLogMessage> _messageQueue;
        private readonly Thread _outputThread;

        private readonly DiscordSocketClient _bot;

        private bool _canProcess;

        public DiscordLoggerProcessor(DiscordSocketClient bot, DiscordLoggerOptions options)
        {
            _canProcess = false;
            _bot = bot;
            _bot.Ready += HandleBotReady;

            Options = options;
            _messageQueue = new BlockingCollection<DiscordLogMessage>((int) Options.MaxQueueSize);

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
            if (!_messageQueue.IsAddingCompleted)
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
                WriteMessage(message);
            }
            catch (Exception)
            {
            }
        }

        private bool WriteMessage(DiscordLogMessage entry)
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
                    channel.SendMessageAsync(part).GetAwaiter().GetResult();
                }
            }
            else
            {
                Console.WriteLine("Could not find log channel");
            }

            return true;
        }

        private void ProcessLogQueue()
        {
            try
            {
                int count = 0;
                List<DiscordLogMessage> messagesToSend = new List<DiscordLogMessage>();

                foreach (DiscordLogMessage message in _messageQueue.GetConsumingEnumerable())
                {
                    while (!_canProcess)
                    {
                        Thread.Sleep(10);
                    }

                    DiscordLogMessage? groupedMessage = messagesToSend.FirstOrDefault(x =>
                        x.ChannelId == message.ChannelId && x.GuildId == message.GuildId);
                    if (groupedMessage == null)
                    {
                        groupedMessage = message;
                        messagesToSend.Add(groupedMessage);
                    }
                    else
                    {
                        groupedMessage.Message += "\n" + message.Message;
                    }

                    count++;

                    if (count < Options.MaxGroupSize && _messageQueue.Count >= Options.MinGroupContinue)
                    {
                        continue;
                    }

                    List<DiscordLogMessage> toRemove = new List<DiscordLogMessage>();
                    foreach (DiscordLogMessage logMessage in messagesToSend)
                    {
                        try
                        {
                            if (WriteMessage(logMessage))
                            {
                                toRemove.Add(logMessage);
                            }
                        }
                        catch (Exception)
                        {
                            EnqueueMessage(logMessage);
                        }
                    }

                    messagesToSend.RemoveAll(x => toRemove.Contains(x));
                    count = 0;
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