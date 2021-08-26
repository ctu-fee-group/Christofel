using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Christofel.BaseLib.Database;
using Christofel.BaseLib.Database.Models;
using Christofel.BaseLib.Database.Models.Enums;
using Christofel.BaseLib.Extensions;
using Christofel.BaseLib.Permissions;
using Christofel.CommandsLib;
using Christofel.CommandsLib.Validator;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Christofel.Management.Commands
{
    [Group("permissions")]
    [Description("Manage user and role permissions")]
    [Ephemeral]
    [RequirePermission("management.permissions")]
    [DiscordDefaultPermission(false)]
    public class PermissionCommandsGroup : CommandGroup
    {
        private readonly ILogger<MessageCommandsGroup> _logger;
        private readonly IPermissionService _permissions;
        private ChristofelBaseContext _dbContext;
        private readonly FeedbackService _feedbackService;

        public PermissionCommandsGroup(
            ILogger<MessageCommandsGroup> logger, FeedbackService feedbackService,
            ChristofelBaseContext dbContext, IPermissionService permissions)
        {
            _dbContext = dbContext;
            _feedbackService = feedbackService;
            _logger = logger;
            _permissions = permissions;
        }

        private Result ExactlyOneValidation(string name, IOptional left, IOptional right)
        {
            var validationResult = new CommandValidator()
                .MakeSure(name, (left, right),
                    o => o
                        .Must(x => x.left.HasValue ^ x.right.HasValue)
                        .WithMessage("Exactly one must be specified."))
                .Validate()
                .GetResult();

            return validationResult;
        }

        [Command("grant")]
        [RequirePermission("management.permissions.grant")]
        [Description("Grant specified permission to user or role. Specify either user or role only")]
        public async Task<Result> HandleGrant(
            [Description("Permission to grant to the user or role")]
            string permission,
            [Description("User to assign permission to"), DiscordTypeHint(TypeHint.User)]
            Optional<Snowflake> user = default,
            [Description("Role to assign permission to")]
            Optional<IRole> role = default)
        {
            var validationResult = ExactlyOneValidation("user, role", user, role);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            PermissionAssignment assignment = new PermissionAssignment()
            {
                PermissionName = permission,
                Target = user.HasValue
                    ? new DiscordTarget(user.Value.Value, TargetType.User)
                    : role.Value.ToDiscordTarget()
            };

            try
            {
                _dbContext.Add(assignment);
                await _dbContext.SaveChangesAsync(CancellationToken);
                var feedbackResult = await _feedbackService.SendContextualSuccessAsync(
                    "Permission granted. Refresh will be needed for it to take full effect.", ct: CancellationToken);

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not save the permission");
                var feedbackResult =
                    await _feedbackService.SendContextualErrorAsync(
                        "Permission could not be saved to the database",
                        ct: CancellationToken);

                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult);
            }
        }

        [Command("revoke")]
        [RequirePermission("management.permissions.revoke")]
        [Description(
            "Revoke specified permission. Specify either user or role only. Exact permission must be specified")]
        public async Task<Result> HandleRevoke(
            [Description("Permission to revoke from the user or role")]
            string permission,
            [Description("User to assign permission to"), DiscordTypeHint(TypeHint.User)]
            Optional<Snowflake> user = default,
            [Description("Role to assign permission to")]
            Optional<IRole> role = default)
        {
            var validationResult = ExactlyOneValidation("user, role", user, role);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            Result<IReadOnlyList<IMessage>> feedbackResult;
            try
            {
                DiscordTarget target = user.HasValue
                    ? new DiscordTarget(user.Value.Value, TargetType.User)
                    : role.Value.ToDiscordTarget();
                
                IQueryable<PermissionAssignment> assignments = _dbContext.Permissions
                    .AsQueryable()
                    .WhereTargetEquals(target)
                    .Where(x => x.PermissionName == permission);

                bool deleted = false;
                await foreach (PermissionAssignment assignment in assignments.AsAsyncEnumerable()
                    .WithCancellation(CancellationToken))
                {
                    deleted = true;
                    _dbContext.Remove(assignment);
                }

                if (deleted)
                {
                    await _dbContext.SaveChangesAsync(CancellationToken);
                    feedbackResult =
                        await _feedbackService.SendContextualSuccessAsync("Permission revoked", ct: CancellationToken);
                }
                else
                {
                    feedbackResult =
                        await _feedbackService.SendContextualSuccessAsync("Permission revoked", ct: CancellationToken);
                    feedbackResult =
                        await _feedbackService.SendContextualErrorAsync("Could not find that permission assignment");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not save the permission");
                feedbackResult =
                    await _feedbackService.SendContextualErrorAsync("Could not save the permission");
            }

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        [Command("list")]
        [Description("Show list of all permissions that are currently loaded in Christofel")]
        [RequirePermission("management.permissions.list")]
        public async Task<Result> HandleList()
        {
            string response = "List of all permissions from attached plugins:\n";
            response += string.Join('\n',
                _permissions.Permissions.Select(x =>
                    $@"  - **{x.PermissionName}** - {x.DisplayName} - {x.Description}"));

            var feedbackResult =
                await _feedbackService.SendContextualSuccessAsync(response, ct: CancellationToken);

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        [Command("show")]
        [Description("Show permissions of role or user. For users their role permissions will be shown as well")]
        [RequirePermission("management.permissions.show")]
        public async Task<Result> HandleShow(
            [Description("Show permissions of user and all their roles")]
            Optional<IGuildMember> user = default,
            [Description("Show permissions of role")]
            Optional<IRole> role = default)
        {
            var validationResult = ExactlyOneValidation("user, role", user, role);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var targets = user.HasValue ? user.Value.GetAllDiscordTargets() : new[] { role.Value.ToDiscordTarget() };

            Result<IReadOnlyList<IMessage>> feedbackResult;
            try
            {
                List<string> permissionAssignments = (await _dbContext.Permissions
                        .AsNoTracking()
                        .WhereTargetAnyOf(targets)
                        .ToListAsync(CancellationToken))
                    .GroupBy(x => x.Target)
                    .Select(grouping =>
                        $@"Permission of {(grouping.Key.GetMentionString())}:" +
                        "\n" +
                        string.Join('\n',
                            grouping.Select(perm => $@"  - **{perm.PermissionName.Replace("*", "\\*")}**")))
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

                feedbackResult = await _feedbackService.SendContextualSuccessAsync(response, ct: CancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not get user/role permission from the database");
                feedbackResult =
                    await _feedbackService.SendContextualErrorAsync(
                        "Could not get user/role permission from the database");
            }

            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }
    }
}