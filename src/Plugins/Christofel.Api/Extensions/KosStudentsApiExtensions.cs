//
//  KosStudentsApiExtensions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kos.Abstractions;
using Kos.Atom;
using Kos.Controllers;
using Kos.Data;
using Kos.Extensions;

namespace Christofel.Api.Extensions;

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
        => api.GetExtremeStudentRole(studentRoles, true, ct);

    /// <summary>
    /// Obtain the role that has the oldest start date.
    /// </summary>
    /// <param name="api">The kos api.</param>
    /// <param name="studentRoles">The student roles to look through.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task<Student?> GetOldestStudentRole
    (
        this IKosAtomApi api,
        List<AtomLoadableEntity<Student>>? studentRoles,
        CancellationToken ct = default
    )
        => api.GetExtremeStudentRole(studentRoles, false, ct);

    private static async Task<Student?> GetExtremeStudentRole
    (
        this IKosAtomApi api,
        List<AtomLoadableEntity<Student>>? studentRoles,
        bool latest,
        CancellationToken ct
    )
    {
        Student? student = null;
        DateTime? date = null;

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
            if (date is null || (currentStudentStartDate > date && latest)
                || (currentStudentStartDate < date && !latest))
            {
                student = currentStudent;
                date = currentStudentStartDate;
            }
        }

        return student;
    }
}