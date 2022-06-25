//
//   ResendRuleCommandsGroup.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Christofel.CommandsLib.Permissions;
using Christofel.Management.Database;
using Microsoft.EntityFrameworkCore;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.Management.Commands
{
    /// <summary>
    /// Command group for resendrule command used for resending all messages from one channel to another one.
    /// </summary>
    [Group("resendrule")]
    [Description("Resend messages from one channel to another")]
    [Ephemeral]
    [RequirePermission("management.resendrule")]
    public class ResendRuleCommandsGroup : CommandGroup
    {
        private readonly ManagementContext _dbContext;
        private readonly FeedbackService _feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResendRuleCommandsGroup"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="feedbackService">The feedback service.</param>
        public ResendRuleCommandsGroup(ManagementContext dbContext, FeedbackService feedbackService)
        {
            _dbContext = dbContext;
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// List existing resend rules.
        /// </summary>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("list")]
        [Ephemeral]
        [RequirePermission("management.resendrule.list")]
        [Description("Lists active resend rules.")]
        public async Task<Result> HandleList()
        {
            string resendRules;
            try
            {
                resendRules = string.Join
                (
                    '\n',
                    await _dbContext.ResendRules.Select(x => $"  - From: <#{x.FromChannel}>, To: <#{x.ToChannel}>").ToListAsync(CancellationToken)
                );
            }
            catch (Exception e)
            {
                await _feedbackService.SendContextualErrorAsync("Could not get resend rules from the database.");
                return e;
            }

            string message = $"Automatically resending messages in channels:\n{resendRules}";
            if (resendRules.Length == 0)
            {
                message = "There are currently no active resend rules.";
            }

            var feedbackResult = await _feedbackService.SendContextualInfoAsync(message);
            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        /// <summary>
        /// Add new resend rule.
        /// </summary>
        /// <param name="from">The channel that messages should be resent from.</param>
        /// <param name="to">The channel that messages should be resent to.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("add")]
        [Ephemeral]
        [RequirePermission("management.resendrule.add")]
        [Description("Add resend rule.")]
        public async Task<Result> HandleAdd
        (
            [DiscordTypeHint(TypeHint.Channel), Description("The channel to resend new messages from.")]
            Snowflake from,
            [DiscordTypeHint(TypeHint.Channel), Description("The channel to resend new messages to.")]
            Snowflake to
        )
        {
            try
            {
                var resendRule = new Database.Models.ResendRule
                {
                    FromChannel = from,
                    ToChannel = to
                };
                _dbContext.Add(resendRule);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await _feedbackService.SendContextualErrorAsync("Could not add resend rule to the database.");
                return e;
            }

            var feedbackResult = await _feedbackService.SendContextualSuccessAsync
                ($"The resent rule from <#{from}> to <#{to}> was successfully added.");
            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }

        /// <summary>
        /// Remove existing resend rule.
        /// </summary>
        /// <param name="from">The channel that messages should be resent from.</param>
        /// <param name="to">The channel that messages should be resent to.</param>
        /// <returns>A result that may not have succeeded.</returns>
        [Command("remove")]
        [Ephemeral]
        [RequirePermission("management.resendrule.add")]
        [Description("Add resend rule.")]
        public async Task<Result> HandleRemove
        (
            [DiscordTypeHint(TypeHint.Channel), Description("The from channel to match.")]
            Snowflake from,
            [DiscordTypeHint(TypeHint.Channel), Description("The to channel to match.")]
            Snowflake to
        )
        {
            var removedCount = 0;
            try
            {
                foreach (var resendRule in _dbContext.ResendRules.Where
                    (x => x.FromChannel == from && x.ToChannel == to))
                {
                    removedCount++;
                    _dbContext.Remove(resendRule);
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await _feedbackService.SendContextualErrorAsync("Could not add resend rule to the database.");
                return e;
            }

            var message = $"The resent rule from <#{from}> to <#{to}> was successfully removed.";
            if (removedCount == 0)
            {
                message = "Could not find matching resend rule.";
            }

            var feedbackResult = await _feedbackService.SendContextualInfoAsync(message);
            return feedbackResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(feedbackResult);
        }
    }
}