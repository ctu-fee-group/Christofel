//
//   CurrentSemesterCache.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data;
using Christofel.CoursesLib.Data;
using Kos.Abstractions;
using Kos.Data;
using Kos.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Christofel.CoursesLib.Services;

/// <summary>
/// Cache for current semester.
/// </summary>
public class CurrentSemesterCache
{
    private readonly IMemoryCache _cache;
    private readonly IKosSemestersApi _kosSemestersApi;
    private readonly ILogger<CurrentSemesterCache> _logger;
    private readonly CurrentSemesterCacheOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentSemesterCache"/> class.
    /// </summary>
    /// <param name="cache">The memory cache.</param>
    /// <param name="kosSemestersApi">The kos semesters api.</param>
    /// <param name="options">The caching duration options.</param>
    /// <param name="logger">The logger.</param>
    public CurrentSemesterCache
    (
        IMemoryCache cache,
        IKosSemestersApi kosSemestersApi,
        IOptions<CurrentSemesterCacheOptions> options,
        ILogger<CurrentSemesterCache> logger
    )
    {
        _cache = cache;
        _kosSemestersApi = kosSemestersApi;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Obtain cached current semester.
    /// </summary>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>The current semester.</returns>
    /// <exception cref="DataException">Thrown if current semester cannot be obtained or parsed.</exception>
    public Task<SemesterFilter> GetCurrentSemester(CancellationToken ct = default) =>
        GetCurrentSemester(false, ct);

    private async Task<SemesterFilter> GetCurrentSemester(bool afterRemoved, CancellationToken ct = default)
    {
        var key = "courses::current_semester";

        var (semester, filter) = await _cache.GetOrCreateAsync<(Semester Semester, SemesterFilter Filter)>
        (
            key,
            async (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheDuration);

                var currentSemester = await _kosSemestersApi.GetCurrentSemester(token: ct);
                if (currentSemester is null)
                {
                    throw new DataException("Could not obtain current semester.");
                }

                if (!SemesterFilter.TryParse(currentSemester.Code, out var semester))
                {
                    throw new DataException($"Could not parse SemesterFilter from {currentSemester}");
                }

                return (currentSemester, semester.Value);
            }
        );

        if ((semester.EndDate ?? DateTime.MaxValue) < DateTime.Now)
        {
            if (afterRemoved)
            {
                // Already tried removing, but kosapi returned wrong semester
                _logger.LogWarning
                    ("Tried obtaining new semester from kosapi, but one with ending date in past was returned again.");
            }
            else
            {
                _cache.Remove(key);
                return await GetCurrentSemester(true, ct);
            }
        }

        return filter;
    }
}