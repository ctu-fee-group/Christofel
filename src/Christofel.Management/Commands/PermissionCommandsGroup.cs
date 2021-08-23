using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Christofel.BaseLib.Configuration;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib;
using Discord;
using Discord.Net.Interactions.Abstractions;
using Discord.Net.Interactions.CommandsInfo;
using Discord.Net.Interactions.Executors;
using Discord.Net.Interactions.HandlerCreator;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.Management.Commands
{
    public class PermissionCommandsGroup : ICommandGroup
    {
        private readonly ILogger<MessageCommandsGroup> _logger;
        private readonly ICommandPermissionsResolver<PermissionSlashInfo> _resolver;
        private readonly IPermissionService _permissions;
        private readonly IDbContextFactory<ChristofelBaseContext> _dbContextFactory;

        public PermissionCommandsGroup(
            ICommandPermissionsResolver<PermissionSlashInfo> resolver, ILogger<MessageCommandsGroup> logger,
            IDbContextFactory<ChristofelBaseContext> dbContextFactory, IPermissionService permissions)
        {
            _logger = logger;
            _resolver = resolver;
            _dbContextFactory = dbContextFactory;
            _permissions = permissions;
        }

        public async Task HandleGrant(SocketInteraction command, IMentionable mentionable, string permission,
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
                await command.FollowupChunkAsync(
                    "Permission granted. Refresh will be needed for it to take full effect.",
                    ephemeral: true, options: new RequestOptions() { CancelToken = token });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not save the permission");
                await command.FollowupChunkAsync("Could not save the permission", ephemeral: true,
                    options: new RequestOptions() { CancelToken = token });
            }
        }

        public async Task HandleRevoke(SocketInteraction command, IMentionable mentionable, string permission,
            CancellationToken token = default)
        {
            await using ChristofelBaseContext context = _dbContextFactory.CreateDbContext();
            try
            {
                IQueryable<PermissionAssignment> assignments = context.Permissions
                    .AsQueryable()
                    .WhereTargetEquals(mentionable.ToDiscordTarget())
                    .Where(x => x.PermissionName == permission);

                bool deleted = false;
                await foreach (PermissionAssignment assignment in assignments.AsAsyncEnumerable()
                    .WithCancellation(token))
                {
                    deleted = true;
                    context.Remove(assignment);
                }

                if (deleted)
                {
                    await context.SaveChangesAsync(token);
                    await command.FollowupChunkAsync("Permission revoked", ephemeral: true,
                        options: new RequestOptions() { CancelToken = token });
                }
                else
                {
                    await command.FollowupChunkAsync("Could not find that permission assignment in database",
                        ephemeral: true,
                        options: new RequestOptions() { CancelToken = token });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not save the permission");
                await command.FollowupChunkAsync("Could not save the permission", ephemeral: true,
                    options: new RequestOptions() { CancelToken = token });
            }
        }

        public Task HandleList(SocketInteraction command, CancellationToken token = default)
        {
            string response = "List of all permissions from attached plugins:\n";
            response += string.Join('\n',
                _permissions.Permissions.Select(x =>
                    $@"  - **{x.PermissionName}** - {x.DisplayName} - {x.Description}"));

            return command.FollowupChunkAsync(response, ephemeral: true,
                options: new RequestOptions() { CancelToken = token });
        }

        public async Task HandleShow(SocketInteraction command, IMentionable mentionable,
            CancellationToken token = default)
        {
            await using ChristofelBaseContext context = _dbContextFactory.CreateDbContext();

            IEnumerable<DiscordTarget> targets;
            if (mentionable is IUser user)
            {
                targets = user.GetAllDiscordTargets();
            }
            else
            {
                targets = new[] { mentionable.ToDiscordTarget() };
            }

            try
            {
                List<string> permissionAssignments = (await context.Permissions
                        .AsNoTracking()
                        .WhereTargetAnyOf(targets)
                        .ToListAsync(token))
                    .GroupBy(x => x.Target)
                    .Select(x =>
                        $@"Permission of {(x.Key.GetMentionString())}:" +
                        "\n" +
                        string.Join('\n', x.Select(x => $@"  - **{x.PermissionName.Replace("*", "\\*")}**")))
                    .ToList();

                string response;
                if (permissionAssignments.Count == 0)
                {
                    response = "Specified target does not have any permissions";
                }
                else
                {
                    response = "Showing all permissions: " + string.Join('\n', permissionAssignments);
                }

                await command.FollowupChunkAsync(response, ephemeral: true,
                    options: new RequestOptions() { CancelToken = token });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get user's permission from the database");
                await command.FollowupChunkAsync("Could not get user's permission from the database", ephemeral: true,
                    options: new RequestOptions() { CancelToken = token });
            }
        }

        private SlashCommandOptionBuilder GetGrantSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
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
                );
        }

        private SlashCommandOptionBuilder GetRevokeSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
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
                );
        }

        private SlashCommandOptionBuilder GetShowSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
                .WithName("show")
                .WithDescription("Show permissions of user or role")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("mentionable")
                    .WithDescription("User or role to revoke from")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Mentionable));
        }

        private SlashCommandOptionBuilder GetListSubcommandBuilder()
        {
            return new SlashCommandOptionBuilder()
                .WithName("list")
                .WithDescription("List permissions of all attached plugins")
                .WithType(ApplicationCommandOptionType.SubCommand);
        }

        public Task SetupCommandsAsync(IInteractionHolder holder,
            CancellationToken token = new CancellationToken())
        {
            DiscordInteractionHandler handler = new SubCommandHandlerCreator()
                .CreateHandlerForCommand(
                    ("grant", (CommandDelegate<IMentionable, string>)HandleGrant),
                    ("revoke", (CommandDelegate<IMentionable, string>)HandleRevoke),
                    ("list", (CommandDelegate)HandleList),
                    ("show", (CommandDelegate<IMentionable>)HandleShow)
                );

            PermissionSlashInfoBuilder permissionsBuilder = new PermissionSlashInfoBuilder()
                .WithPermission("management.permissions")
                .WithHandler(handler)
                .WithBuilder(new SlashCommandBuilder()
                    .WithName("permissions")
                    .WithDescription("Manage permissions")
                    .AddOption(GetGrantSubcommandBuilder())
                    .AddOption(GetRevokeSubcommandBuilder())
                    .AddOption(GetShowSubcommandBuilder())
                    .AddOption(GetListSubcommandBuilder())
                );

            IInteractionExecutor executor = new InteractionExecutorBuilder<PermissionSlashInfo>()
                .WithLogger(_logger)
                .WithPermissionCheck(_resolver)
                .WithThreadPool()
                .WithDeferMessage()
                .Build();

            holder.AddInteraction(permissionsBuilder.Build(), executor);
            return Task.CompletedTask;
        }
    }
}