using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Api.Ctu.Auth.Tasks;
using HotChocolate.Types;
using Kos;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;
using RTools_NTS.Util;
using Usermap;

namespace Christofel.Api.Ctu.Auth.Steps
{
    public class SetNicknameAuthStep : IAuthStep
    {
        private readonly AuthorizedUsermapApi _usermapApi;
        private readonly AuthorizedKosApi _kosApi;
        
        public SetNicknameAuthStep(AuthorizedUsermapApi usermapApi, AuthorizedKosApi kosApi)
        {
            _usermapApi = usermapApi;
            _kosApi = kosApi;
        }

        public async Task<Result> FillDataAsync(IAuthData data, CancellationToken ct = default)
        {
            var nickname = (await GetNicknameFromUsermap(data, ct)) ?? (await GetNicknameFromKos(data, ct));
            data.StepData.Add("Nickname", nickname);

            return Result.FromSuccess();
        }

        private async Task<string?> GetNicknameFromKos(IAuthData data, CancellationToken ct)
        {
            var kosPerson = await _kosApi.People.GetPersonAsync(data.LoadedUser.CtuUsername, token: ct);

            return kosPerson is null
                ? null
                : GetNickname(kosPerson.FirstName, kosPerson.LastName, GetCurrentUsername(data.GuildUser));
        }
        
        private async Task<string?> GetNicknameFromUsermap(IAuthData data, CancellationToken ct)
        {
            var usermapPerson = await _usermapApi.People.GetPersonAsync(data.LoadedUser.CtuUsername, token: ct);

            return usermapPerson is null
                ? null
                : GetNickname(usermapPerson.FirstName, usermapPerson.LastName, GetCurrentUsername(data.GuildUser));
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