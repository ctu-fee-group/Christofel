//
//   DateTimeProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Options;

namespace Christofel.Helpers.Date;

/// <summary>
/// Default DateTimeProvider with time zone options.
/// </summary>
public class DateTimeProvider : IDateTimeProvider, IDisposable
{
    private readonly IDateTimeBaseProvider _baseProvider;
    private readonly IDisposable? _optionsMonitorToken;
    private TimeOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeProvider"/> class.
    /// </summary>
    /// <param name="baseProvider">The provider of UtcNow and SystemNow.</param>
    /// <param name="options">The options denoting preferred timezone.</param>
    public DateTimeProvider
        (
            IDateTimeBaseProvider baseProvider,
            IOptionsMonitor<TimeOptions> options
        )
    {
        _baseProvider = baseProvider;
        _options = options.CurrentValue;
        _optionsMonitorToken = options.OnChange(o => _options = o);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _optionsMonitorToken?.Dispose();
    }

    /// <inheritdoc/>
    public DateTime UtcNow => _baseProvider.UtcNow;

    /// <inheritdoc/>
    public DateTimeOffset SystemNow
    {
        get
        {
            var systemNow = _baseProvider.SystemNow;
            var utcNow = UtcNow;
            var offset = systemNow - utcNow;

            return new DateTimeOffset(utcNow, offset);
        }
    }

    /// <inheritdoc/>
    public DateTimeOffset PreferredNow
    {
        get
        {
            var tz = PreferredTimeZone;
            var utcNow = UtcNow;
            var convertedNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
            return new DateTimeOffset(convertedNow, tz.GetUtcOffset(convertedNow));
        }
    }

    /// <inheritdoc/>
    public TimeZoneInfo PreferredTimeZone => TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZone);
}
