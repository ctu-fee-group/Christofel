using System;
using System.IO;
using System.Threading.Tasks;
using Christofel.CommandsLib.Verifier;
using Christofel.CommandsLib.Verifier.Interfaces;
using Christofel.Messages.Services;
using Discord;
using Newtonsoft.Json;

namespace Christofel.Messages.Commands.Verifiers
{
    public static class EmbedCommandVerifierExtensions
    {
        public static CommandVerifier<T> VerifyIsEmbedJson<T>(this CommandVerifier<T> verifier, EmbedsProvider embeds,
            string embed,
            string parameterName = "embed")
            where T : class, IHasEmbed, new()
        {
            verifier.QueueWork(() => verifier.VerifyIsEmbedJsonAsync(embeds, embed, parameterName));
            return verifier;
        }

        public static CommandVerifier<T> VerifyFileIsEmbedJson<T>(this CommandVerifier<T> verifier,
            EmbedsProvider embeds,
            string fileName,
            string parameterName = "embed")
            where T : class, IHasEmbed, new()
        {
            verifier.QueueWork(() => verifier.VerifyFileIsEmbedJsonAsync(embeds, fileName, parameterName));
            return verifier;
        }

        private static Task<CommandVerifier<T>> VerifyIsEmbedJsonAsync<T>(this CommandVerifier<T> verifier,
            EmbedsProvider embeds, string embed, string parameterName = "embed")
            where T : class, IHasEmbed, new()
        {
            try
            {
                verifier.Result.Embed = embeds.GetEmbedFromString(embed);
            }
            catch (ArgumentException e)
            {
                verifier.SetFailed(parameterName, $@"{e.ParamName}: {e.Message}");
            }
            catch (JsonException e)
            {
                verifier.SetFailed(parameterName, e.Message);
            }

            return Task.FromResult(verifier);
        }

        private static async Task<CommandVerifier<T>> VerifyFileIsEmbedJsonAsync<T>(this CommandVerifier<T> verifier,
            EmbedsProvider embeds, string fileName, string parameterName = "embed")
            where T : class, IHasEmbed, new()
        {
            if (!verifier.Success)
            {
                return verifier;
            }
            
            try
            {
                verifier.Result.Embed = await embeds.GetEmbedFromFile(fileName);
            }
            catch (ArgumentException e)
            {
                verifier.SetFailed(parameterName, $@"{e.ParamName}: {e.Message}");
            }
            catch (JsonException e)
            {
                verifier.SetFailed(parameterName, e.Message);
            }

            return verifier;
        }
    }
}