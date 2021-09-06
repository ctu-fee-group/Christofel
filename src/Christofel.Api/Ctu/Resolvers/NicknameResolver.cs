using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.User;
using Kos.Abstractions;
using Remora.Discord.API.Abstractions.Objects;
using Usermap.Controllers;

namespace Christofel.Api.Ctu.Resolvers
{
    public class NicknameResolver
    {
        private readonly IKosPeopleApi _kosPeopleApi;
        private readonly IUsermapPeopleApi _usermapPeopleApi;

        public NicknameResolver(IKosPeopleApi kosPeopleApi, IUsermapPeopleApi usermapPeopleApi)
        {
            _kosPeopleApi = kosPeopleApi;
            _usermapPeopleApi = usermapPeopleApi;
        }

        public async Task<string?> ResolveNicknameAsync(ILinkUser user, IGuildMember guildMember, CancellationToken ct = default)
        {
            return
                (await GetNicknameFromUsermap(user, guildMember, ct)) ?? 
                (await GetNicknameFromKos(user, guildMember, ct));
        }

        private async Task<string?> GetNicknameFromKos(ILinkUser user, IGuildMember guildMember, CancellationToken ct)
        {
            var kosPerson = await _kosPeopleApi.GetPersonAsync(user.CtuUsername, token: ct);

            return kosPerson is null
                ? null
                : GetNickname(kosPerson.FirstName, kosPerson.LastName, GetCurrentUsername(guildMember));
        }

        private async Task<string?> GetNicknameFromUsermap(ILinkUser user, IGuildMember guildMember, CancellationToken ct)
        {
            var usermapPerson = await _usermapPeopleApi.GetPersonAsync(user.CtuUsername, token: ct);

            return usermapPerson is null
                ? null
                : GetNickname(usermapPerson.FirstName, usermapPerson.LastName, GetCurrentUsername(guildMember));
        }


        private string? GetCurrentUsername(IGuildMember member)
        {
            if (member.Nickname.IsDefined(out var nickname))
            {
                return nickname;
            }

            if (member.User.IsDefined(out var user))
            {
                return user.Username;
            }

            return null;
        }

        private string GetNickname(string firstName, string lastName, string? discordUsername)
        {
            var nickname = $"{firstName.Split(' ', 2).FirstOrDefault()} {lastName.FirstOrDefault()}.";

            if (discordUsername is null || nickname.Length + discordUsername.Length > 30)
            {
                return nickname;
            }

            return $"{nickname} ({discordUsername})";
        }
    }
}