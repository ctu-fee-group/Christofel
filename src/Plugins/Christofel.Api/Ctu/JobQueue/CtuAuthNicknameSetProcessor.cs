using System.Threading.Tasks;
using Christofel.Plugins.Lifetime;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;

namespace Christofel.Api.Ctu.JobQueue
{
    public record CtuAuthNicknameSet(Snowflake UserId, Snowflake GuildId, string Nickname);

    public class CtuAuthNicknameSetProcessor : ThreadPoolJobQueue<CtuAuthNicknameSet>
    {
        private readonly ILogger _logger;
        private readonly ILifetime _lifetime;
        private readonly IDiscordRestGuildAPI _guildApi;

        public CtuAuthNicknameSetProcessor(ICurrentPluginLifetime lifetime, ILogger<CtuAuthNicknameSetProcessor> logger,
            IDiscordRestGuildAPI guildApi)
            : base(lifetime, logger)
        {
            _logger = logger;
            _lifetime = lifetime;
            _guildApi = guildApi;
        }

        protected override async Task ProcessAssignJob(CtuAuthNicknameSet job)
        {
            var modifiedResult = await _guildApi.ModifyGuildMemberAsync(job.GuildId, job.UserId, nickname: job.Nickname,
                ct: _lifetime.Stopping);

            if (!modifiedResult.IsSuccess)
            {
                _logger.LogWarning(
                    $"Could not change nickname of <@{job.UserId}>, not going to retry. {modifiedResult.Error.Message}");
            }
        }
    }
}