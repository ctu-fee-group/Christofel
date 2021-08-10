using Newtonsoft.Json;

namespace Christofel.Api.Discord
{
    public record DiscordUser(
        [JsonProperty("id")] ulong Id,
        [JsonProperty("username")] string Username,
        [JsonProperty("discriminator")] string Discriminator,
        [JsonProperty("avatar")] string? Avatar,
        [JsonProperty("bot")] bool? Bot,
        [JsonProperty("system")] bool? System,
        [JsonProperty("mfa_enabled")] bool? MfaEnabled,
        [JsonProperty("locale")] string? Locale,
        [JsonProperty("verified")] bool? Verified,
        [JsonProperty("flags")] int? Flags,
        [JsonProperty("premium_type")] int? PremiumType,
        [JsonProperty("public_flags")] int? PublicFlags
    );
}