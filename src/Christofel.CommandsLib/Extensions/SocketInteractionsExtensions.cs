using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Christofel.CommandsLib.Extensions
{
    public static class SocketInteractionsExtensions
    {
        public static async Task<RestInteractionMessage> EditResponseAsync(this SocketInteraction interaction, string? text = null, Embed[]? embeds = null,
            bool ephemeral = false, AllowedMentions? allowedMentions = null, RequestOptions? options = null, MessageComponent? component = null)
        {
            RestInteractionMessage originalResponse = await interaction.GetOriginalResponseAsync();
            await originalResponse.ModifyAsync(props =>
            {
                props.AllowedMentions = allowedMentions;
                props.Components = component;
                props.Embeds = embeds;
                props.Content = text;
                props.Flags = ephemeral ? MessageFlags.Ephemeral : MessageFlags.None;
            }, options);

            return originalResponse;
        }
    }
}