using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.Rest;

namespace Christofel.CommandsLib.Commands
{
    /// <summary>
    /// Thread-safe implementation of CommandHolder
    /// </summary>
    public class CommandHolder : ICommandHolder
    {
        private readonly List<ICommandHolder.HeldSlashCommand> _commands;
        private readonly object _commandsLock = new object();
        
        public CommandHolder(DiscordRestClient client, IPermissionService permissions)
        {
            _commands = new List<ICommandHolder.HeldSlashCommand>();
        }

        public IEnumerable<ICommandHolder.HeldSlashCommand> Commands
        {
            get
            {
                lock (_commandsLock)
                {
                    return _commands.ToImmutableList();
                }
            }
        }

        public ICommandHolder.HeldSlashCommand? TryGetSlashCommand(string name)
        {
            return _commands.FirstOrDefault(x => x.Info.BuiltCommand.Name == name);
        }

        public SlashCommandInfo AddCommand(SlashCommandInfoBuilder builder, ICommandExecutor executor)
        {
            SlashCommandInfo info = builder.Build();

            lock (_commandsLock)
            {
                _commands.Add(new ICommandHolder.HeldSlashCommand(info, executor));
            }

            return info;
        }

        public void RemoveCommands()
        {
            lock (_commandsLock)
            {
                _commands.Clear();
            }
        }
    }
}