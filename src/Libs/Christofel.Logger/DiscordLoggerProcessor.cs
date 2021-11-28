//
//   DiscordLoggerProcessor.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Christofel.Logger
{
    /// <summary>
    /// Background worker processing queue of discord logs.
    /// </summary>
    public class DiscordLoggerProcessor : IDisposable
    {
        // TODO: how to handle exceptions here?
        private readonly BlockingCollection<DiscordLogMessage> _messageQueue;
        private readonly Thread _outputThread;
        private readonly IServiceProvider _provider;
        private IDiscordRestChannelAPI? _channelApi;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordLoggerProcessor"/> class.
        /// </summary>
        /// <param name="provider">The provider of the services.</param>
        /// <param name="options">The options of the provider.</param>
        public DiscordLoggerProcessor(IServiceProvider provider, DiscordLoggerOptions options)
        {
            _provider = provider;

            Options = options;
            _messageQueue = new BlockingCollection<DiscordLogMessage>((int)Options.MaxQueueSize);

            _outputThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true, Name = "Discord logger queue processing thread",
            };
            _outputThread.Start();
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        public DiscordLoggerOptions Options { get; internal set; }

        /// <inheritdoc />
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

        /// <summary>
        /// Enqueues message to the queue.
        /// </summary>
        /// <remarks>
        /// If the collection is full or it was already completed,
        /// the message will be sent right away blocking the thread.
        /// </remarks>
        /// <param name="message">The message to enqueue.</param>
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
                // ignored
            }
        }

        private static IEnumerable<string> Chunk(string? str, int chunkSize) =>
            !string.IsNullOrEmpty(str)
                ? Enumerable.Range(0, (int)Math.Ceiling((double)str.Length / chunkSize))
                    .Select
                    (
                        i => str
                            .Substring
                            (
                                i * chunkSize,
                                (i * chunkSize) + chunkSize <= str.Length ? chunkSize : str.Length - (i * chunkSize)
                            )
                    )
                : Enumerable.Empty<string>();

        private bool SendMessage(DiscordLogMessage entry)
        {
            if (_disposed)
            {
                return false;
            }

            _channelApi ??= _provider.GetRequiredService<IDiscordRestChannelAPI>();

            var success = true;
            Optional<IMessageReference> message = default;
            foreach (string part in Chunk(entry.Message, 2000))
            {
                var result =
                    _channelApi.CreateMessageAsync
                    (
                        new Snowflake(entry.ChannelId, Constants.DiscordEpoch),
                        part,
                        messageReference: message,
                        allowedMentions:
                        new AllowedMentions
                        (
                            Roles: new List<Snowflake>(),
                            Users: new List<Snowflake>()
                        )
                    ).GetAwaiter().GetResult();

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
            var retries = 5;
            foreach (DiscordLogMessage message in messages)
            {
                var sent = false;

                while (!sent && retries-- > 0)
                {
                    try
                    {
                        sent = SendMessage(message);
                    }
                    catch (Exception)
                    { // Generally exceptions shouldn't happen here
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
                while (!_messageQueue.IsCompleted)
                {
                    List<DiscordLogMessage> fetchedMessages =
                        _messageQueue.FetchAtLeastOneBlocking(Options.MaxGroupSize);

                    List<DiscordLogMessage> messagesToSend = fetchedMessages
                        .GroupBy(x => new { x.GuildId, x.ChannelId })
                        .Select
                        (
                            x =>
                                new DiscordLogMessage
                                (
                                    x.Key.GuildId,
                                    x.Key.ChannelId,
                                    string.Join('\n', x.Select(message => message.Message))
                                )
                        ).ToList();

                    SendMessages(messagesToSend);
                }
            }
            catch (Exception)
            {
                try
                {
                    _messageQueue.CompleteAdding();
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}