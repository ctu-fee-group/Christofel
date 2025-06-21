//
//   DateTimeProviderTests.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Helpers.Date;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Christofel.Helpers.Tests;

/// <summary>
/// Tests for <see cref="DateTimeProvider" />.
/// </summary>
public class DateTimeProviderTests
{
    private class DateTimeBaseProviderStub : IDateTimeBaseProvider
    {
        public DateTime UtcNow { get; set; }

        public DateTime SystemNow { get; set; }
    }

    /// <summary>
    /// Tests that the SystemNow will be returned as UtcNow with proper offset.
    /// </summary>
    [Fact]
    public void ReturnsProperOffsetBetweenUtcAndSystemTime()
    {
        var stub = new DateTimeBaseProviderStub();
        var options = new TimeOptions();
        stub.UtcNow = new DateTime(2024, 11, 12, 0, 0, 0);
        stub.SystemNow = new DateTime(2024, 11, 12, 2, 0, 0);

        IServiceProvider services = new ServiceCollection()
            .AddLogging(b => b.ClearProviders())
            .Configure<TimeOptions>(o => o.TimeZone = "Europe/Prague")
            .AddSingleton<IDateTimeBaseProvider>(_ => stub)
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .BuildServiceProvider();

        var provider = services.GetRequiredService<IDateTimeProvider>();

        var systemNow = provider.SystemNow;

        Assert.Equal(TimeSpan.FromHours(2), systemNow.Offset);
        Assert.Equal(stub.SystemNow, systemNow.DateTime);
    }

    /// <summary>
    /// Tests that the returned timezone is the one requested by options.
    /// </summary>
    [Fact]
    public void ReturnsPreferredTimeZone()
    {
        var stub = new DateTimeBaseProviderStub();
        var options = new TimeOptions();
        stub.UtcNow = new DateTime(2024, 11, 12, 0, 0, 0);
        stub.SystemNow = new DateTime(2024, 11, 12, 2, 0, 0);

        IServiceProvider services = new ServiceCollection()
            .AddLogging(b => b.ClearProviders())
            .Configure<TimeOptions>(o => o.TimeZone = "Europe/Prague")
            .AddSingleton<IDateTimeBaseProvider>(_ => stub)
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .BuildServiceProvider();

        var provider = services.GetRequiredService<IDateTimeProvider>();

        Assert.Equal(TimeSpan.FromHours(1), provider.PreferredNow.Offset);
        Assert.Equal(TimeSpan.FromHours(1), provider.PreferredTimeZone.BaseUtcOffset);
        Assert.Equal(TimeSpan.FromHours(1), provider.PreferredTimeZone.GetUtcOffset(stub.UtcNow));
    }

    /// <summary>
    /// Tests that the returned timezone is the one requested by options.
    /// </summary>
    [Fact]
    public void ReturnsPreferredNow()
    {
        var stub = new DateTimeBaseProviderStub();
        var options = new TimeOptions();
        stub.UtcNow = new DateTime(2024, 11, 12, 0, 0, 0);
        stub.SystemNow = new DateTime(1970, 1, 1, 0, 0, 0);

        IServiceProvider services = new ServiceCollection()
            .AddLogging(b => b.ClearProviders())
            .Configure<TimeOptions>(o => o.TimeZone = "Europe/Prague")
            .AddSingleton<IDateTimeBaseProvider>(_ => stub)
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .BuildServiceProvider();

        var provider = services.GetRequiredService<IDateTimeProvider>();

        // now not daylight savings
        var now = provider.PreferredNow;
        Assert.Equal(TimeSpan.FromHours(1), now.Offset);
        Assert.Equal(stub.UtcNow.AddHours(1), now.DateTime);

        // now daylight savings
        stub.UtcNow = new DateTime(2025, 5, 23, 16, 23, 0);
        now = provider.PreferredNow;
        Assert.Equal(TimeSpan.FromHours(2), now.Offset);
        Assert.Equal(stub.UtcNow.AddHours(2), now.DateTime);
    }
}
