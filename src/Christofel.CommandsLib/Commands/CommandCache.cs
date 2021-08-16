using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Christofel.CommandsLib.Commands
{
    public class CommandCache
    {
        private readonly DiscordRestClient _client;
        private ulong? _applicationId;

        private IReadOnlyCollection<RestGlobalCommand>? _cachedGlobalCommands;
        private ConcurrentDictionary<ulong, IReadOnlyCollection<RestGuildCommand>>? _cachedGuildCommands;

        public CommandCache(DiscordRestClient client)
        {
            _client = client;
        }

        public void Reset()
        {
            _cachedGlobalCommands = null;
            _cachedGuildCommands = null;
        }

        public async Task<IReadOnlyCollection<RestGuildCommand>> GetGuildCommands(ulong guildId,
            CancellationToken token = default)
        {
            if (_cachedGuildCommands == null)
            {
                _cachedGuildCommands = new ConcurrentDictionary<ulong, IReadOnlyCollection<RestGuildCommand>>();
            }

            if (!_cachedGuildCommands.TryGetValue(guildId, out IReadOnlyCollection<RestGuildCommand>? guildCommands))
            {
                ulong applicationId = await GetApplicationId(token);
                guildCommands = (await _client.GetGuildApplicationCommands(guildId,
                        options: new RequestOptions() {CancelToken = token}))
                    .Where(x => x.ApplicationId == applicationId)
                    .ToList();
                _cachedGuildCommands.TryAdd(guildId, guildCommands);
            }

            return guildCommands;
        }

        public async Task<IReadOnlyCollection<RestGlobalCommand>> GetGlobalCommands(CancellationToken token = default)
        {
            if (_cachedGlobalCommands == null)
            {
                ulong applicationId = await GetApplicationId(token);
                _cachedGlobalCommands =
                    (await _client.GetGlobalApplicationCommands(options: new RequestOptions() {CancelToken = token}))
                        .Where(x => x.ApplicationId == applicationId).ToList();
            }

            return _cachedGlobalCommands;
        }

        public async Task<RestGuildCommand?> GetGuildCommand(ulong guildId, string name,
            CancellationToken token = default)
        {
            return (await GetGuildCommands(guildId, token))
                .FirstOrDefault(x => x.Name == name);
        }

        public async Task<RestGlobalCommand?> GetGlobalCommand(string name, CancellationToken token = default)
        {
            return (await GetGlobalCommands(token))
                .FirstOrDefault(x => x.Name == name);
        }

        private async Task<ulong> GetApplicationId(CancellationToken token = default)
        {
            _applicationId ??= (await _client.GetApplicationInfoAsync()).Id;
            return (ulong) _applicationId;
        }
    }
}