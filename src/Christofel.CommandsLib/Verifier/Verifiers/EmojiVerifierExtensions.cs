using System;
using System.Threading.Tasks;
using Christofel.CommandsLib.Verifier.Interfaces;
using Discord;

namespace Christofel.CommandsLib.Verifier.Verifiers
{
    public static class EmojiVerifierExtensions
    {
        public static CommandVerifier<T> VerifyEmote<T>(this CommandVerifier<T> verifier, string emojiString,
            string parameterName = "emoji")
            where T : new()
        {
            verifier.QueueWork(() => verifier.VerifyEmoteAsync(emojiString, parameterName));
            return verifier;
        }

        private static Task<CommandVerifier<T>> VerifyEmoteAsync<T>(this CommandVerifier<T> verifier,
            string emojiString, string parameterName = "emoji")
            where T : new()
        {
            if (!(verifier.Result is IHasEmote hasEmote))
            {
                throw new InvalidOperationException(
                    "Cannot set Emote as the type does not implement IHasEmote");
            }
            
            IEmote? outEmote = null;
            if (outEmote == null && Emote.TryParse(emojiString, out Emote emote))
            {
                outEmote = emote;
            }
            
            if (outEmote == null && Emoji.TryParse(emojiString, out Emoji emoji))
            {
                outEmote = emoji;
            }

            if (outEmote == null)
            {
                verifier.SetFailed(parameterName, "Emoji cannot be parsed.");
                return Task.FromResult(verifier);
            }

            hasEmote.Emote = outEmote;

            return Task.FromResult(verifier);
        }
    }
}