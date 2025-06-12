//
//   CronRepository.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.Helpers.Cron;

/// <summary>
/// Repository holding Cron jobs.
/// </summary>
public class CronRepository
{
    private readonly Dictionary<string, Type> _crons;

    /// <summary>
    /// Initializes a new instance of the <see cref="CronRepository"/> class.
    /// </summary>
    public CronRepository()
    {
        _crons = new Dictionary<string, Type>();
    }

    /// <summary>
    /// Registers a cron job in repository's dictionary.
    /// </summary>
    /// <typeparam name="TCron">The type of cron to register.</typeparam>
    public void RegisterCron<TCron>()
        where TCron : ICronJob
    {
        _crons.Add(TCron.Name, typeof(TCron));
    }

    /// <summary>
    /// Returns a cron job with the given name, if available, null otherwise.
    /// </summary>
    /// <param name="name">Name of the cron to look for.</param>
    /// <returns>The cron job type, if found, null otherwise.</returns>
    public Type? TryGetCronType(string name) =>
        _crons.ContainsKey(name) ? _crons[name] : null;

    /// <summary>
    /// Returns a cron job with the given name, if available, null otherwise.
    /// </summary>
    /// <param name="services">The service provider to look for.</param>
    /// <param name="name">Name of the cron to look for.</param>
    /// <returns>The cron job, if found, null otherwise.</returns>
    public ICronJob? TryGetCron(IServiceProvider services, string name)
    {
        var type = TryGetCronType(name);

        return type is null
            ? null
            : (ICronJob)services.GetRequiredService(type);
    }

    /// <summary>
    /// Gets the names of the crons that are available.
    /// </summary>
    public IEnumerable<string> Crons => _crons.Keys;
}
