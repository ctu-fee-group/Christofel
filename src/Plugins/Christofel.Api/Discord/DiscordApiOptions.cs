namespace Christofel.Api.Discord
{
    /// <summary>
    /// Options used while calling Discord API
    /// </summary>
    public class DiscordApiOptions
    {
        /// <summary>
        /// What url is the API located at
        /// </summary>
        public string BaseUrl { get; set; } = "https://discord.com/api/v9";
    }
}