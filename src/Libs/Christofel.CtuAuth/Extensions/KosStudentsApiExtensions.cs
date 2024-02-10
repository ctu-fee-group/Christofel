//
//  KosStudentsApiExtensions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Kos.Abstractions;
using Kos.Atom;
using Kos.Controllers;
using Kos.Data;
using Kos.Extensions;
using Remora.Rest.Core;

namespace Christofel.CtuAuth.Extensions;

/// <summary>
/// Extension methods for <see cref="KosStudentsApi"/>.
/// </summary>
public static class KosStudentsApiExtensions
{
    /// <summary>
    /// Obtain the role that has the latest start date.
    /// </summary>
    /// <param name="api">The kos api.</param>
    /// <param name="studentRoles">The student roles to look through.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task<Student?> GetLatestStudentRole
    (
        this IKosAtomApi api,
        List<AtomLoadableEntity<Student>>? studentRoles,
        CancellationToken ct = default
    )
        => api.GetExtremeStudentRole(studentRoles, true, ct: ct);

    /// <summary>
    /// Obtain the role that has the oldest start date.
    /// </summary>
    /// <param name="api">The kos api.</param>
    /// <param name="studentRoles">The student roles to look through.</param>
    /// <param name="groupBy">Group found extremes, return the student from most recent group. Example usage is to get only the last faculty, instead of all studies on CTU.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task<Student?> GetOldestStudentRole
    (
        this IKosAtomApi api,
        List<AtomLoadableEntity<Student>>? studentRoles,
        Optional<Func<Student, string>> groupBy = default,
        CancellationToken ct = default
    )
        => api.GetExtremeStudentRole(studentRoles, false, groupBy, ct);

    private static async Task<Student?> GetExtremeStudentRole
    (
        this IKosAtomApi api,
        List<AtomLoadableEntity<Student>>? studentRoles,
        bool latest,
        Optional<Func<Student, string>> groupBy = default,
        CancellationToken ct = default
    )
    {
        var groups = new Dictionary<string, (Student Student, DateTime StartDate)>();

        if (studentRoles is null)
        {
            return null;
        }

        foreach (var studentRole in studentRoles)
        {
            var currentStudent = await api.LoadEntityContentAsync(studentRole, token: ct);

            if (currentStudent is null)
            {
                continue;
            }

            var currentStudentStartDate = currentStudent.StartDate ?? DateTime.Today;
            var group = groupBy.HasValue ? groupBy.Value(currentStudent) : string.Empty;

            if (!groups.TryGetValue(group, out var groupedExtreme))
            {
                groups.Add(group, (currentStudent, currentStudentStartDate));
            }
            else if ((currentStudentStartDate > groupedExtreme.StartDate && latest)
                || (currentStudentStartDate < groupedExtreme.StartDate && !latest))
            {
                groups[group] = (currentStudent, currentStudentStartDate);
            }
        }

        // Get the latest group (usually faculty)
        return groups.Count > 0 ?
            groups.Values.MaxBy(x => x.StartDate).Student
            : null;
    }
}