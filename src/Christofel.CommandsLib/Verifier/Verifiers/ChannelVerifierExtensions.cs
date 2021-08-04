using System;
using System.Threading.Tasks;
using Christofel.CommandsLib.Verifier.Interfaces;
using Discord;

namespace Christofel.CommandsLib.Verifier.Verifiers
{
    public static class ChannelVerifierExtensions
    {
        /// <summary>
        /// Returns MessageChannel from CommandVerifier if T implements IHasMessageChannel and channel is set
        /// Throws InvalidOperationException on failure of getting the MessageChannel
        /// </summary>
        /// <param name="verifier"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IMessageChannel GetMessageChannel<T>(this CommandVerifier<T> verifier)
            where T : IHasMessageChannel, new()
        {
            if (verifier.Result.Channel == null)
            {
                throw new InvalidOperationException(
                    "IMessageChannel was expected, but is null");
            }

            return verifier.Result.Channel;
        }

        /// <summary>
        /// Verifies whether the specified channel is a IMessageChannel
        /// T must implement IHasMessageChannel
        /// </summary>
        /// <param name="verifier"></param>
        /// <param name="channel"></param>
        /// <param name="parameterName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CommandVerifier<T> VerifyMessageChannel<T>(this CommandVerifier<T> verifier, IChannel? channel,
            string parameterName = "channel")
            where T : class, IHasMessageChannel, new()
        {
            verifier.QueueWork(() => verifier.VerifyMessageChannelAsync<T>(channel, parameterName));
            return verifier;
        }

        private static Task<CommandVerifier<T>> VerifyMessageChannelAsync<T>(this CommandVerifier<T> verifier, IChannel? channel,
            string parameterName = "channel")
            where T : class, IHasMessageChannel, new()
        {
            if (!(channel is IMessageChannel messageChannel))
            {
                verifier.SetFailed(parameterName, "Specified channel is not a text channel.");
                return Task.FromResult(verifier);
            }

            verifier.Result.Channel = messageChannel;
            return Task.FromResult(verifier);
        }
    }
}