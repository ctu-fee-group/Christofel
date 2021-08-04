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
        public static ulong GetMessageId<T>(this CommandVerifier<T> verifier)
            where T : new()
        {
            if (!(verifier.Result is IHasMessageId hasMessageId) || hasMessageId.MessageId == null)
            {
                throw new InvalidOperationException("Cannot find id of the message");
            }

            return (ulong) hasMessageId.MessageId;
        }

        public static IUserMessage GetUserMessage<T>(this CommandVerifier<T> verifier)
            where T : new()
        {
            if (!(verifier.Result is IHasRestUserMessage hasRestMessage) || hasRestMessage.UserMessage == null)
            {
                throw new InvalidOperationException("Cannot find rest message");
            }

            return hasRestMessage.UserMessage;
        }

        public static CommandVerifier<T> VerifyMessageId<T>(this CommandVerifier<T> verifier,
            string messageId,
            string parameterName = "messageid")
            where T : new()
        {
            verifier.QueueWork(() => verifier.VerifyMessageIdAsync(messageId, parameterName));
            return verifier;
        }

        public static CommandVerifier<T> VerifyMessageAuthorChristofel<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid")
            where T : new()
        {
            verifier.QueueWork(() => verifier.VerifyMessageAuthorChristofelAsync(parameterName));
            return verifier;
        }

        public static CommandVerifier<T> VerifyRestUserMessage<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid", CancellationToken token = default)
            where T : new()
        {
            verifier.QueueWork(() => verifier.VerifyUserMessageAsync(parameterName, token));
            return verifier;
        }

        private static Task<CommandVerifier<T>> VerifyMessageIdAsync<T>(this CommandVerifier<T> verifier,
            string messageId,
            string parameterName = "messageid")
            where T : new()
        {
            if (!(verifier.Result is IHasMessageId hasMessageId))
            {
                throw new InvalidOperationException(
                    "Cannot set MessageId as the type does not implement IHasMessageId");
            }

            if (!ulong.TryParse(messageId, out ulong messageIdUlong))
            {
                verifier.SetFailed(parameterName, "Message id could not be converted.");
            }

            hasMessageId.MessageId = messageIdUlong;
            return Task.FromResult(verifier);
        }

        private static async Task<CommandVerifier<T>> VerifyUserMessageAsync<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid", CancellationToken token = default)
            where T : new()
        {
            if (!verifier.Success)
            {
                return verifier;
            }

            IMessageChannel messageChannel = verifier.GetMessageChannel();
            ulong messageId = verifier.GetMessageId();

            if (!(verifier.Result is IHasRestUserMessage hasUserMessage))
            {
                throw new InvalidOperationException(
                    "Cannot set RestUserMessage as the type does not implement IHasRestUserMessage.");
            }

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

            hasUserMessage.UserMessage = userMessage;

            return verifier;
        }

        private static Task<CommandVerifier<T>> VerifyMessageAuthorChristofelAsync<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid")
            where T : new()
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