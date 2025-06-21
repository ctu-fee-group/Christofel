//
//   SelfManagementCommandsTests.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Helpers.Date;
using Christofel.Management.Commands;
using static Christofel.Management.Commands.SelfManagementCommands;

namespace Christofel.Management.Tests;

/// <summary>
/// Tests for <see cref="SelfManagementCommands" />.
/// </summary>
/// <remarks>
/// Currently only tests the date times for timeout until specifications.
/// </remarks>
public class SelfManagementCommandsTests
{
    /// <summary>
    /// Gets a set of various test cases.
    /// </summary>
    /// <remarks>
    /// Elements consist of [expected, preferredNow, specification].
    /// </remarks>
    public static IEnumerable<object[]> Cases =>
    [

        // Generic tests
        [
            new DateTimeOffset(new DateTime(2025, 6, 11, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 10, 13, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfDay
        ],
        [
            new DateTimeOffset(new DateTime(2025, 6, 11, 6, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 10, 13, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.FollowingMorning
        ],
        [
            new DateTimeOffset(new DateTime(2025, 6, 16, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 10, 13, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfWeek
        ],
        [
            new DateTimeOffset(new DateTime(2025, 6, 14, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 10, 13, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfWorkWeek
        ],
        [
            new DateTimeOffset(new DateTime(2025, 7, 1, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 10, 13, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfMonth
        ],
        [
            new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 12, 10, 13, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfMonth
        ],

        // Edge cases for EndOfDay (close to midnight, timezone goes through midnight)
        [
            new DateTimeOffset(new DateTime(2025, 6, 11, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 10, 23, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfDay
        ],
        [
            new DateTimeOffset(new DateTime(2025, 6, 11, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 10, 0, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfDay
        ],

        // Edge cases for Following Morning

        // 3 AM
        [
            new DateTimeOffset(new DateTime(2025, 6, 10, 6, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 10, 3, 0, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.FollowingMorning
        ],

        // 7 AM
        [
            new DateTimeOffset(new DateTime(2025, 6, 11, 6, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 10, 7, 0, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.FollowingMorning
        ],

        // Edge cases for EndOfWorkWeek

        // Friday 23 59
        [
            new DateTimeOffset(new DateTime(2025, 6, 14, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 13, 23, 59, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfWorkWeek
        ],

        // Sunday
        [
            new DateTimeOffset(new DateTime(2025, 6, 21, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 15, 12, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfWorkWeek
        ],

        // Monday
        [
            new DateTimeOffset(new DateTime(2025, 6, 21, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 16, 0, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfWorkWeek
        ],

        // Edge cases for EndOfWeek

        // Sunday
        [
            new DateTimeOffset(new DateTime(2025, 6, 16, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 15, 23, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfWeek
        ],

        // Monday
        [
            new DateTimeOffset(new DateTime(2025, 6, 23, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 6, 16, 0, 30, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfWeek
        ],

        // Edge cases for EndOfMonth

        // February
        [
            new DateTimeOffset(new DateTime(2025, 3, 1, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2025, 2, 28, 23, 59, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfMonth
        ],

        // February leap year
        [
            new DateTimeOffset(new DateTime(2024, 3, 1, 0, 0, 0), TimeSpan.FromHours(2)),
            new DateTimeOffset(new DateTime(2024, 2, 29, 23, 59, 0), TimeSpan.FromHours(2)),
            TimeoutUntilSpecification.EndOfMonth
        ],
    ];

    private class DateTimeProviderStub : IDateTimeProvider
    {
        // Shouldn't be used.
        public DateTime UtcNow => throw new NotImplementedException();

        // Shouldn't be used.
        public DateTimeOffset SystemNow => throw new NotImplementedException();

        public DateTimeOffset PreferredNow { get; set; }

        // Shouldn't be used.
        public TimeZoneInfo PreferredTimeZone => throw new NotImplementedException();
    }

    /// <summary>
    /// Tests various cases of TimeoutUntilSpecification for regular cases and edge cases.
    /// </summary>
    /// <param name="expected">Expected result for the given arguments.</param>
    /// <param name="preferredNow">The PrefferedNow to set in the date time provider.</param>
    /// <param name="specification">The specification argument to the method under test.</param>
    [Theory]
    [MemberData(nameof(Cases))]
    public void SpecificationToDateTimeOffsetReturnsProperDateTimes
        (
            DateTimeOffset expected,
            DateTimeOffset preferredNow,
            TimeoutUntilSpecification specification
        )
    {
        var stub = new DateTimeProviderStub
        {
            PreferredNow = preferredNow
        };

        var selfManagementCommands = new SelfManagementCommands
        (
            null!,
            null!,
            null!,
            stub,
            null!
        );

        var result = selfManagementCommands.SpecificationToDateTimeOffset(specification);
        Assert.Equal(expected, result);
    }
}
