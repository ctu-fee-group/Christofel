using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Extensions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Christofel.CommandsLib.Extensions
{
    public static class InteractionExtensions
    {
        public static int MaxMessageLength = 2000;
        
        public static async Task RespondChunkAsync(this SocketInteraction interaction, string? text = null, Embed[]? embeds = null, bool isTTS = false,
            bool ephemeral = false, AllowedMentions? allowedMentions = null, RequestOptions? options = null, MessageComponent? component = null)
        {
            IEnumerable<string> chunked = text.Chunk(MaxMessageLength).ToList();

            await interaction
                .RespondAsync(text: chunked.FirstOrDefault(), embeds, isTTS, ephemeral, allowedMentions, options,
                    component);
            
            foreach (string chunk in chunked.Skip(1))
            {
                await interaction
                    .FollowupAsync(chunk, null, isTTS, ephemeral, allowedMentions, options, null);
            }
        }
        
        public static async Task<RestFollowupMessage> FollowupChunkAsync(this SocketInteraction interaction, string? text = null, Embed[]? embeds = null, bool isTTS = false,
            bool ephemeral = false, AllowedMentions? allowedMentions = null, RequestOptions? options = null, MessageComponent? component = null)
        {
            List<string> chunked = text.Chunk(MaxMessageLength).ToList();

            RestFollowupMessage last;
            last = await interaction
                .FollowupAsync(text: chunked.FirstOrDefault(), embeds, isTTS, ephemeral, allowedMentions, options,
                    component);
            
            foreach (string chunk in chunked.Skip(1))
            {
                last = await interaction
                    .FollowupAsync(chunk, null, isTTS, ephemeral, allowedMentions, options, null);
            }

            return last;
        }
    }
}