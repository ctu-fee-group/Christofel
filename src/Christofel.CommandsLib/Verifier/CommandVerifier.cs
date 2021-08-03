using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Christofel.CommandsLib.Verificator
{
    public record Verified<T> (T Result, bool Success, IEnumerable<VerifyFailMessage> FailMessages);

    public record VerifyFailMessage (string ParameterName, string Message);

    public class CommandVerifier<T>
        where T : new()
    {
        private Task _task;
        private List<Task> _tasks;
        private List<VerifyFailMessage> _failMessages;
        private bool _firstResponse;
        
        public CommandVerifier(DiscordSocketClient client, SocketSlashCommand command, ILogger logger, bool firstResponse = true)
        {
            _tasks = new List<Task>();
            _task = Task.FromResult<int>(0);
            _failMessages = new List<VerifyFailMessage>();
            _firstResponse = firstResponse;

            Result = new T();
            Success = true;

            Client = client;
            Command = command;
            Logger = logger;
        }
        
        public DiscordSocketClient Client { get; }
        
        public ILogger Logger { get; }

        public SocketSlashCommand Command { get; }
        
        public T Result { get; }

        public bool Success { get; private set; }

        public void SetFailed(string parameterName, string message)
        {
            Success = false;
            _failMessages.Add(new VerifyFailMessage(parameterName, message));
        }

        public async Task<Verified<T>> FinishVerificationAsync()
        {
            // Wait for any work to finish
            await Task.WhenAll(_tasks);

            if (!Success)
            {
                StringBuilder responseMessage = new StringBuilder("Command verification failed. Problems:\n");
                
                foreach (VerifyFailMessage message in _failMessages)
                {
                    responseMessage.AppendLine($@"  - Parameter '{message.ParameterName}' failed with: {message.Message}");
                }

                string response = responseMessage.ToString();
                Logger.LogWarning($@"Command {Command.Data.Name} executed by {Command.User}: " + response);
                if (_firstResponse)
                {
                    await Command.RespondAsync(response, ephemeral: true);
                }
                else
                {
                    await Command.FollowupAsync(response, ephemeral: true);
                }
            }
            
            return new Verified<T>(Result, Success, _failMessages);
        }

        public void QueueWork(Func<Task> work)
        {
            _tasks.Add(_task = _task.
                ContinueWith<CommandVerifier<T>>(task =>
                {
                    work().GetAwaiter().GetResult();
                    return this;
                }, TaskContinuationOptions.OnlyOnRanToCompletion));
        }
    }
}