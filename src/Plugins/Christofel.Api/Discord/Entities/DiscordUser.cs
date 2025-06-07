//
//   DiscordUser.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Christofel.Api.Discord
{
    /// <summary>
    /// User result obtained from Discord API v9
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Username"></param>
    /// <param name="Discriminator"></param>
    /// <param name="Avatar"></param>
    /// <param name="Bot"></param>
    /// <param name="System"></param>
    /// <param name="MfaEnabled"></param>
    /// <param name="Locale"></param>
    /// <param name="Verified"></param>
    /// <param name="Flags"></param>
    /// <param name="PremiumType"></param>
    /// <param name="PublicFlags"></param>
    public record DiscordUser
    (
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