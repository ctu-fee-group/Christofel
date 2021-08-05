using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib.Commands;
using Christofel.CommandsLib.CommandsInfo;
using Christofel.CommandsLib.Executors;
using Christofel.CommandsLib.HandlerCreator;
using Christofel.CommandsLib.Extensions;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Management.Commands
{
    public class PermissionCommandsGroup : ICommandGroup
    {
        private readonly DiscordSocketClient _client;
        private readonly BotOptions _options;
        private readonly ILogger<MessageCommandsGroup> _logger;
        private readonly IPermissionsResolver _resolver;
        private readonly IPermissionService _permissions;
        private readonly IDbContextFactory<ChristofelBaseContext> _dbContextFactory;

        public PermissionCommandsGroup(IOptions<BotOptions> options, IPermissionsResolver resolver,
            ILogger<MessageCommandsGroup> logger, DiscordSocketClient client,
            IDbContextFactory<ChristofelBaseContext> dbContextFactory, IPermissionService permissions)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
            _resolver = resolver;
            _dbContextFactory = dbContextFactory;
            _permissions = permissions;
        }

        public async Task HandleGrant(SocketSlashCommand command, IMentionable mentionable, string permission,
            CancellationToken token = default)
        {
            await using ChristofelBaseContext context = _dbContextFactory.CreateDbContext();
            PermissionAssignment assignment = new PermissionAssignment()
            {
                PermissionName = permission,
                Target = mentionable.ToDiscordTarget()
            };

            try
            {
                context.Add(assignment);
                await context.SaveChangesAsync(token);
                await command.RespondAsync("Permission granted. Refresh will be needed for it to take full effect.");
            }
            catch (Exception)
            {
                await command.RespondAsync("Could not save the permission");
                throw;
            }
        }

        public async Task HandleRevoke(SocketSlashCommand command, IMentionable mentionable, string permission,
            CancellationToken token = default)
        {
            await using ChristofelBaseContext context = _dbContextFactory.CreateDbContext();
            try
            {
                PermissionAssignment? assignment = await context.Permissions.AsQueryable().FirstOrDefaultAsync(x =>
                    x.Target == mentionable.ToDiscordTarget() && x.PermissionName == permission, token);

                if (assignment == null)
                {
                    await command.RespondAsync("Could not find that permission assignment in database");
                }
                else
                {
                    context.Remove(assignment);
                    await context.SaveChangesAsync(token);
                    await command.RespondAsync("Permission revoked");
                }
            }
            catch (Exception)
            {
                await command.RespondAsync("Could not save the permission");
                throw;
            }
        }

        public Task HandleList(SocketSlashCommand command, CancellationToken token = default)
        {
            string response = "List of all permissions from attached plugins:\n";
            response += string.Join('\n',
                _permissions.Permissions.Select(x => $@"  - {x.PermissionName} - {x.DisplayName} - {x.Description}"));

            return command.RespondAsync(response, ephemeral: true, options: new RequestOptions() {CancelToken = token});
        }

        public async Task HandleShow(SocketSlashCommand command, IMentionable mentionable,
            CancellationToken token = default)
        {
            await using ChristofelBaseContext context = _dbContextFactory.CreateDbContext();

            DiscordTarget target = mentionable.ToDiscordTarget();
            string response = $@"Permissions of {mentionable.Mention}:";
            response += string.Join('\n', await context.Permissions
                .AsNoTracking()
                .Where(x => x.Target == target)
                .Select(x => $@"  - {x.PermissionName}")
                .ToListAsync(token));

            await command.RespondAsync(response, ephemeral: true, options: new RequestOptions() {CancelToken = token});
        }

        public Task SetupCommandsAsync(ICommandHolder holder, CancellationToken token = new CancellationToken())
        {
            SlashCommandHandler handler = new SubCommandHandlerCreator()
                .CreateHandlerForCommand(
                    ("grant", (CommandDelegate<IMentionable, string>) HandleGrant),
                    ("revoke", (CommandDelegate<IMentionable, string>) HandleRevoke),
                    ("list", (CommandDelegate) HandleList),
                    ("show", (CommandDelegate<IMentionable>) HandleShow)
                );

            SlashCommandInfoBuilder permissionsBuilder = new SlashCommandInfoBuilder()
                .WithGuild(_options.GuildId)
                .WithPermission("management.permissions")
                .WithHandler(handler)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("permissions")
                    .WithDescription("Manage permissions")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("grant")
                        .WithDescription("Assign permission to user or role")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("mentionable")
                            .WithDescription("User or role to assign to")
                            .WithRequired(true)
                            .WithType(ApplicationCommandOptionType.Mentionable))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("permission")
                            .WithRequired(true)
                            .WithDescription("Permission to assign")
                            .WithType(ApplicationCommandOptionType.String)
                        )
                    )
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("revoke")
                        .WithDescription("Revoke (remove already assigned) permission from user or role")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("mentionable")
                            .WithDescription("User or role to revoke from")
                            .WithRequired(true)
                            .WithType(ApplicationCommandOptionType.Mentionable))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("permission")
                            .WithRequired(true)
                            .WithDescription("Permission to revoke")
                            .WithType(ApplicationCommandOptionType.String)
                        )
                    )
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("show")
                        .WithDescription("Show permissions of user or role")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("mentionable")
                            .WithDescription("User or role to revoke from")
                            .WithRequired(true)
                            .WithType(ApplicationCommandOptionType.Mentionable))
                    )
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("list")
                        .WithDescription("List permissions of all attached plugins")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                    )
                );

            ICommandExecutor executor = new CommandExecutorBuilder()
                .WithLogger(_logger)
                .WithThreadPool()
                .Build();

            holder.AddCommand(permissionsBuilder, executor);
            return Task.CompletedTask;
        }
    }
}