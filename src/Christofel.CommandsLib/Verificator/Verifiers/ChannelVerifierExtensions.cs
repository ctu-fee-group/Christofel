using System;
using System.Threading.Tasks;
using Christofel.CommandsLib.Verificator.Interfaces;
using Discord;

namespace Christofel.CommandsLib.Verificator.Verifiers
{
    public static class ChannelVerifierExtensions
    {
        public static IMessageChannel GetMessageChannel<T>(this CommandVerifier<T> verifier)
            where T : new()
        {
            if (!(verifier.Result is IHasMessageChannel hasMessageChannel) || hasMessageChannel.Channel == null)
            {
                throw new InvalidOperationException(
                    "Cannot find channel. The parameter should inherit IHasMessageChannel and message channel should not be null.");
            }

            return hasMessageChannel.Channel;
        }

        public static CommandVerifier<T> VerifyMessageChannel<T>(this CommandVerifier<T> verifier, IChannel? channel,
            string parameterName = "channel")
            where T : new()
        {
            verifier.QueueWork(() => verifier.VerifyMessageChannelAsync<T>(channel, parameterName));
            return verifier;
        }

        private static Task<CommandVerifier<T>> VerifyMessageChannelAsync<T>(this CommandVerifier<T> verifier, IChannel? channel,
            string parameterName = "channel")
            where T : new()
        {
            if (!(verifier.Result is IHasMessageChannel hasMessageChannel))
            {
                throw new InvalidOperationException(
                    "Cannot set the message channel as the type does not implement IHasMessageChannel");
            }
            
            if (!(channel is IMessageChannel messageChannel))
            {
                verifier.SetFailed(parameterName, "Specified channel is not a text channel.");
                return Task.FromResult(verifier);
            }

            hasMessageChannel.Channel = messageChannel;
            return Task.FromResult(verifier);
        }
    }
}