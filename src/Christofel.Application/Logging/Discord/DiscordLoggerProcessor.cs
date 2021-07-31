using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Discord;
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
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(message);
                    return;
                }
                catch (InvalidOperationException) { }
            }

            // Adding is completed so just log the message
            try
            {
                WriteMessage(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        static IEnumerable<string> Split(string str, int chunkSize) =>
            Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));

        private void WriteMessage(DiscordLogMessage entry)
        {
            var channel = _bot
                .GetGuild(entry.GuildId)
                .GetTextChannel(entry.ChannelId);

            if (channel != null)
            {
                foreach (string part in Split(entry.Message, 2000))
                {
                    channel.SendMessageAsync(part).GetAwaiter().GetResult();
                }
            }
            else
            {
                Console.WriteLine("Could not find log channel");
            }
        }

        private void ProcessLogQueue()
        {
            try
            {
                foreach (DiscordLogMessage message in _messageQueue.GetConsumingEnumerable())
                {
                    while (!_canProcess)
                    {
                        Thread.Sleep(10);
                    }
                    
                    try
                    {
                        WriteMessage(message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        EnqueueMessage(message);
                    }
                }
            }
            catch
            {
                try
                {
                    _messageQueue.CompleteAdding();
                }
                catch { }
            }
        }

        public void Dispose()
        {
            _messageQueue.CompleteAdding();

            try
            {
                _outputThread.Join(1500);
            }
            catch (ThreadStateException) { }
        }

        protected Task HandleBotReady()
        {
            _canProcess = true;
            return Task.CompletedTask;
        }
    }
}