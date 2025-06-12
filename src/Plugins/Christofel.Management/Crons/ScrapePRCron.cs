//
//   ScrapePRCron.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Christofel.Common.Database;
using Christofel.Common.Database.Models;
using Christofel.Helpers.Cron;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Results;

namespace Christofel.Management.Crons;

/// <summary>
/// Scrapes PR usernames from the website, and populates
/// <see cref="UsernameRoleAssignment"/>.
/// </summary>
public class ScrapePRCron : SimpleCronJob, IDisposable, ICronJob
{
    private readonly IDbContextFactory<ChristofelBaseContext> _dbContextFactory;
    private readonly ILogger _logger;
    private readonly IDisposable? _onChangeToken;
    private PRScrapingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrapePRCron"/> class.
    /// </summary>
    /// <param name="dbContextFactory">Factory for creating the ChristofelBaseContext.</param>
    /// <param name="logger">Logger to log information and errors with.</param>
    /// <param name="options">The options for sraping, css query, the assignment to add etc.</param>
    public ScrapePRCron
        (
            IDbContextFactory<ChristofelBaseContext> dbContextFactory,
            ILogger<ScrapePRCron> logger,
            IOptionsMonitor<PRScrapingOptions> options
        )
        : base
        (
            "ScrapePRCron",
            logger,
            TimeSpan.FromDays(7)
        )
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _options = options.CurrentValue;
        _onChangeToken = options.OnChange(o => _options = o);
    }

    /// <inheritdoc cref="ICronJob" />
    static string ICronJob.Name => "ScrapePRCron";

    /// <inheritdoc/>
    public void Dispose()
    {
        _onChangeToken?.Dispose();
    }

    /// <inheritdoc/>
    protected override async Task<IResult> ProcessAsync(bool manual)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(_options.Url);

        var newUsernames = document.QuerySelectorAll(_options.EmailQuery)
            .Select(x => x.GetAttribute("href"))
            .Where(x => x?.StartsWith("mailto:") ?? false)
            .Cast<string>()
            .Select(x => x.Substring(7).Split("@")[0])
            .ToList();

        using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var current = await dbContext.UsernameRoleAssignment
                .Where(x => x.AssignmentId == _options.AssignmentRoleId)
                .ToListAsync();

            var currentUsernames = current.Select(x => x.Username);

            var toRemove = currentUsernames.Except(newUsernames);
            var toAdd = newUsernames.Except(currentUsernames);

            dbContext.UsernameRoleAssignment
                .RemoveRange(current
                             .Where(x => toRemove.Contains(x.Username)));
            foreach (var username in toAdd)
            {
                dbContext.UsernameRoleAssignment.Add
                    (
                        new UsernameRoleAssignment
                        {
                            Username = username,
                            AssignmentId = _options.AssignmentRoleId
                        }
                    );
            }

            await dbContext.SaveChangesAsync();
        }

        return Result.FromSuccess();
    }
}
