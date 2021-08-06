using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Christofel.CommandsLib.Extensions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Christofel.CommandsLib.Verifier
{
    /// <summary>
    /// Data received from CommandVerifier.
    /// If Success is false, data did not pass the verification
    /// FailMessages contains messages with reasons of fail, Result contains
    /// the requested type
    /// </summary>
    /// <param name="Result"></param>
    /// <param name="Success"></param>
    /// <param name="FailMessages"></param>
    /// <typeparam name="T"></typeparam>
    public record Verified<T> (T Result, bool Success, IEnumerable<VerifyFailMessage> FailMessages);

    /// <summary>
    /// Message for verifier saying what has failed to verify
    /// </summary>
    /// <param name="ParameterName">Parameter that is incorrect</param>
    /// <param name="Message">Message to show (what has gone wrong)</param>
    public record VerifyFailMessage (string ParameterName, string Message);

    /// <summary>
    /// Class used for veryfying commands
    /// It should be used along with extension methods
    /// that provide verification
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandVerifier<T>
        where T : new()
    {
        private Task _task;
        private List<Task> _tasks;
        private List<VerifyFailMessage> _failMessages;
        private bool _firstResponse;

        /// <summary>
        /// Create CommandVerifier
        /// </summary>
        /// <param name="client">Client to use for various api calls</param>
        /// <param name="command">What command data are being verified</param>
        /// <param name="logger">What logger to log into verification problems</param>
        /// <param name="firstResponse">If true, RespondAsync on the command will be used to report verification problems, if false, FollowupAsycn will be used instead</param>
        public CommandVerifier(DiscordSocketClient client, SocketSlashCommand command, ILogger logger,
            bool firstResponse = true)
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

        /// <summary>
        /// Socket client for various api calls
        /// </summary>
        public DiscordSocketClient Client { get; }

        /// <summary>
        /// Logger to log into from verifiers or about failed verification summary
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// What command is being verified
        /// </summary>
        public SocketSlashCommand Command { get; }

        /// <summary>
        /// Result of the verification with correct data set
        /// </summary>
        public T Result { get; }

        /// <summary>
        /// If the verification is successful so far
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Sets that the verification has failed (Success to false)
        /// and adds a message for specified parameter name
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="message"></param>
        public void SetFailed(string parameterName, string message)
        {
            Success = false;
            _failMessages.Add(new VerifyFailMessage(parameterName, message));
        }

        /// <summary>
        /// Runs all jobs that need to run, sends response to the user on failure
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Verified<T>> FinishVerificationAsync()
        {
            // Wait for any work to finish
            try
            {
                await Task.WhenAll(_tasks);
            }
            catch (Exception)
            {
                await Respond("There was an error, sorry. Check the log.");
                throw;
            }

            if (!Success)
            {
                StringBuilder responseMessage = new StringBuilder("Command verification failed. Problems:\n");

                foreach (VerifyFailMessage message in _failMessages)
                {
                    responseMessage.AppendLine(
                        $@"  - Parameter '{message.ParameterName}' failed with: {message.Message}");
                }

                string response = responseMessage.ToString();
                Logger.LogWarning($@"Command {Command.Data.Name} executed by {Command.User}: " + response);
                await Respond(response);
            }

            return new Verified<T>(Result, Success, _failMessages);
        }

        private Task Respond(string response)
        {
            return _firstResponse
                ? Command.RespondChunkAsync(response, ephemeral: true)
                : Command.FollowupChunkAsync(response, ephemeral: true);
        }

        public void QueueWork(Func<Task> work)
        {
            _tasks.Add(_task = _task.ContinueWith<CommandVerifier<T>>(task =>
            {
                work().GetAwaiter().GetResult();
                return this;
            }, TaskContinuationOptions.OnlyOnRanToCompletion));
        }
    }
}