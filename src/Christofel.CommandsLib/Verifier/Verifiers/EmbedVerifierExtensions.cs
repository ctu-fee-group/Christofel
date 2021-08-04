using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace Christofel.CommandsLib.Verifier.Verifiers
{
    public static class EmbedVerifierExtensions
    {
        public static CommandVerifier<T> VerifyUserMessageHasEmbeds<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid")
            where T : new()
        {
            verifier.QueueWork(() => verifier.VerifyUserMessageHasEmbedsAsync(parameterName));
            return verifier;
        }

        private static Task<CommandVerifier<T>> VerifyUserMessageHasEmbedsAsync<T>(this CommandVerifier<T> verifier,
            string parameterName = "messageid")
            where T : new()
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