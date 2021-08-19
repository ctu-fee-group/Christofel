using System.Threading.Tasks;
using Discord;
using Discord.Net.Interactions.Verifier;

namespace Christofel.Management.Commands.Verifiers
{
    public static class UserVerifierExtensions
    {
        public static CommandVerifier<T> VerifyUserOrUserId<T>(this CommandVerifier<T> verifier,
            IUser? user,
            string? discordId,
            string parameterName = "user, discordid")
            where T : class, IHasDiscordId, new()
        {
            verifier.QueueWork(() => verifier.VerifyUserOrUserIdAsync(user, discordId, parameterName));
            return verifier;
        }

        private static Task VerifyUserOrUserIdAsync<T>(this CommandVerifier<T> verifier,
            IUser? user,
            string? discordId,
            string parameterName = "user, discordid")
            where T : class, IHasDiscordId, new()
        {
            if (user != null)
            {
                verifier.Result.DiscordId = user.Id;
            }
            else if (discordId != null && ulong.TryParse(discordId, out ulong id))
            {
                verifier.Result.DiscordId = id;
            }

            if (verifier.Result.DiscordId == null)
            {
                verifier.SetFailed(parameterName, "User or discord id is wrong or missing");
            }

            return Task.CompletedTask;
        }
    }
}