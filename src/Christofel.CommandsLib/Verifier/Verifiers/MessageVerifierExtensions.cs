using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.CommandsLib.Verifier.Interfaces;
using Discord;
using Discord.Rest;

namespace Christofel.CommandsLib.Verifier.Verifiers
{
    public static class MessageVerifierExtensions
    {
        /// <summary>
        /// Returns ulong MessageId from CommandVerifier if T implements IHasMessageId and messageId is set
        /// Throws InvalidOperationException on failure of getting the message id
        /// </summary>
        /// <param name="verifier"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static ulong GetMessageId<T>(this CommandVerifier<T> verifier)
            where T : IHasMessageId, new()
        {
            if (verifier.Result.MessageId == null)
            {
                throw new InvalidOperationException("MessageId should not be null");
            }

            return (ulong) verifier.Result.MessageId;
        }

        /// <summary>
        /// Returns UserMessage from CommandVerifier if T implements IHasUserMessage and user message is set
        /// Throws InvalidOperationException on failure of getting the user message
        /// </summary>
        /// <param name="verifier"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IUserMessage GetUserMessage<T>(this CommandVerifier<T> verifier)
            where T : IHasUserMessage, new()
        {
            if (verifier.Result.UserMessage == null)
            {
                throw new InvalidOperationException("UserMessage should not be null");
            }

            return verifier.Result.UserMessage;
        }

        /// <summary>
        /// Verifies that messageId is ulong and sets MessageId to the id
        /// </summary>
        /// <param name="verifier"></param>
        /// <param name="messageId"></param>
        /// <param name="parameterName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CommandVerifier<T> VerifyMessageId<T>(this CommandVerifier<T> verifier,
            string messageId,
            string parameterName = "messageid")
            where T : class, IHasMessageId, new()
        {
            verifier.QueueWork(() => verifier.VerifyMessageIdAsync(messageId, parameterName));
            return verifier;
        }

        /// <summary>
        /// Verifies that the author of the IUserMessage is Christofel
        /// UserMessage has to be set beforehand, that means calling <see cref="VerifyUserMessage"/>
        /// </summary>
        /// <param name="verifier"></param>
        /// <param name="parameterName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CommandVerifier<T> VerifyMessageAuthorChristofel<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid")
            where T : class, IHasUserMessage, new()
        {
            verifier.QueueWork(() => verifier.VerifyMessageAuthorChristofelAsync(parameterName));
            return verifier;
        }

        /// <summary>
        /// Verifies that the author of the IUserMessage is Christofel
        /// MessageId has to be set beforehand, that means calling <see cref="VerifyMessageId"/>
        /// MessageChannel has to be set beforehand, that means calling <see cref="ChannelVerifierExtensions.VerifyMessageChannel"/>
        /// /// </summary>
        /// <param name="verifier"></param>
        /// <param name="parameterName"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CommandVerifier<T> VerifyUserMessage<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid", CancellationToken token = default)
            where T : class, IHasMessageId, IHasUserMessage, IHasMessageChannel, new()
        {
            verifier.QueueWork(() => verifier.VerifyUserMessageAsync(parameterName, token));
            return verifier;
        }

        private static Task<CommandVerifier<T>> VerifyMessageIdAsync<T>(this CommandVerifier<T> verifier,
            string messageId,
            string parameterName = "messageid")
            where T : class, IHasMessageId, new()
        {
            if (!ulong.TryParse(messageId, out ulong messageIdUlong))
            {
                verifier.SetFailed(parameterName, "Message id could not be converted.");
            }

            verifier.Result.MessageId = messageIdUlong;
            return Task.FromResult(verifier);
        }

        private static async Task<CommandVerifier<T>> VerifyUserMessageAsync<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid", CancellationToken token = default)
            where T : class, IHasMessageId, IHasUserMessage, IHasMessageChannel, new()
        {
            if (!verifier.Success)
            {
                return verifier;
            }

            IMessageChannel messageChannel = verifier.GetMessageChannel();
            ulong messageId = verifier.GetMessageId();

            IMessage? message =
                await messageChannel.GetMessageAsync(messageId, options: new RequestOptions() {CancelToken = token});

            if (message == null)
            {
                verifier.SetFailed(parameterName, "Message could not be found.");
                return verifier;
            }

            if (!(message is IUserMessage userMessage))
            {
                verifier.SetFailed(parameterName, "Message could not be matched to a user message.");
                return verifier;
            }

            verifier.Result.UserMessage = userMessage;

            return verifier;
        }

        private static Task<CommandVerifier<T>> VerifyMessageAuthorChristofelAsync<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid")
            where T : IHasUserMessage, new()
        {
            if (!verifier.Success)
            {
                return Task.FromResult(verifier);
            }

            IUserMessage message = verifier.GetUserMessage();

            if (message.Author.Id != verifier.Client.CurrentUser.Id)
            {
                verifier.SetFailed(parameterName, "Author of the message is not this bot.");
            }

            return Task.FromResult(verifier);
        }
    }
}