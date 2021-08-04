using System.Linq;
using System.Threading.Tasks;
using Christofel.CommandsLib.Verifier.Interfaces;
using Discord;

namespace Christofel.CommandsLib.Verifier.Verifiers
{
    public static class EmbedVerifierExtensions
    {
        /// <summary>
        /// Verify what there is any embed in UserMessage.
        /// The UserMessage must be set beforehand. That means calling VerifyUserMessage<T> before this verifier.
        /// </summary>
        /// <param name="verifier"></param>
        /// <param name="parameterName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CommandVerifier<T> VerifyUserMessageHasEmbeds<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid")
            where T : IHasUserMessage, new()
        {
            verifier.QueueWork(() => verifier.VerifyUserMessageHasEmbedsAsync(parameterName));
            return verifier;
        }
        
        private static Task<CommandVerifier<T>> VerifyUserMessageHasEmbedsAsync<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid")
            where T : IHasUserMessage, new()
        {
            if (!verifier.Success)
            {
                return Task.FromResult(verifier);
            }

            IUserMessage message = verifier.GetUserMessage();
            if (message.Embeds.Count == 0)
            {
                verifier.SetFailed(parameterName, "Message does not contain embeds");
            }
            
            return Task.FromResult(verifier);
        }
    }
}