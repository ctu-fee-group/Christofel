using System;
using System.Collections.Concurrent;
using System.Threading;
using Christofel.BaseLib.Discord;

namespace Christofel.Application.Logging.Discord
{
    public class DiscordLoggerProcessor : IDisposable
    {
        private const int _maxQueuedMessages = 1024;

        private readonly BlockingCollection<DiscordLogMessage> _messageQueue = new BlockingCollection<DiscordLogMessage>(_maxQueuedMessages);
        private readonly Thread _outputThread;

        private readonly IBot _bot;

        public DiscordLoggerProcessor(IBot bot)
        {
            _bot = bot;
            _outputThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "Discord logger queue processing thread"
            };
            _outputThread.Start();
        }

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
            catch (Exception)
            {
                
            }
        }

        private void WriteMessage(DiscordLogMessage entry)
        {
            _bot.Client
                .GetGuild(entry.GuildId)
                .GetTextChannel(entry.ChannelId)
                .SendMessageAsync(entry.Message).GetAwaiter().GetResult();
        }

        private void ProcessLogQueue()
        {
            try
            {
                foreach (DiscordLogMessage message in _messageQueue.GetConsumingEnumerable())
                {
                    WriteMessage(message);
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
    }
}