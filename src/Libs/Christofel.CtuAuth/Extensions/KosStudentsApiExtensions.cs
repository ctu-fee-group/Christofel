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
    /// Obtain the programme, taking into account that the programme doesn't have to be unique.
    /// </summary>
    /// <param name="api">The kos api.</param>
    /// <param name="programme">The programme to load.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task<(Programme? Programme, bool Unique)> GetNonUniqueProgramme
    (
        this IKosProgrammesApi api,
        AtomLoadableEntity<Programme> programme,
        CancellationToken ct = default
    )
    {
        var programmes = await api.GetProgrammes(
            query: $"code=='{programme.GetKey()}'",
            limit: 20,
            token: ct
        );

        // Normally, ignore name of the programme. But if there are more,
        // also filter based on the name. This is a fail safe mechanism,
        // where if for whatever reason the title didn't match the programme,
        // programme is going to be matched anyway. For that reason, do not
        // filter by name if only one programme matched. (ie. it's omitted from the query)
        if (programmes.Count > 1)
        {
            var newProgrammes = programmes
                .Where(p => p.Name == programme.Title)
                .ToArray();

            if (newProgrammes.Length >= 1)
            {
                programmes = newProgrammes;
            }
        }

        return (programmes.FirstOrDefault(), programmes.Count <= 1);
    }

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

    /// <summary>
    /// Obtains Student roles from the given list that are still
    /// active (not terminated). The StudyState is checked for Closed,
    /// and the EndDate is checked. The EndDate has to be null or in the future,
    /// the studystate cannot be closed.
    /// </summary>
    /// <param name="api">The kos api.</param>
    /// <param name="studentRoles">The student roles to look through.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task<IReadOnlyList<Student>> GetActiveStudentRoles
        (
            this IKosAtomApi api,
            List<AtomLoadableEntity<Student>>? studentRoles,
            CancellationToken ct = default
        )
        => await api.FilterStudentRoles
        (
            studentRoles,
            (student) => student.StudyState != StudyState.Closed && (student.EndDate is null || student.EndDate > DateTime.Now),
            ct
        );

    /// <summary>
    /// Obtains Student roles from the given list that are are past graduation.
    /// Meaning the StudyTerminationReason is Graduation.
    /// </summary>
    /// <param name="api">The kos api.</param>
    /// <param name="studentRoles">The student roles to look through.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task<IReadOnlyList<Student>> GetGraduatedStudentRoles
        (
            this IKosAtomApi api,
            List<AtomLoadableEntity<Student>>? studentRoles,
            CancellationToken ct = default
        )
        => await api.FilterStudentRoles
        (
            studentRoles,
            (student) => student.StudyTerminationReason == StudyTermination.Graduation,
            ct
        );

    private static async Task<IReadOnlyList<Student>> FilterStudentRoles
        (
            this IKosAtomApi api,
            List<AtomLoadableEntity<Student>>? studentRoles,
            Func<Student, bool> filter,
            CancellationToken ct = default
        )
    {
        if (studentRoles is null)
        {
            return Array.Empty<Student>();
        }

        var activeStudents = new List<Student>();
        foreach (var studentRole in studentRoles)
        {
            var currentStudent = await api.LoadEntityContentAsync(studentRole, token: ct);

            if (currentStudent is null)
            {
                continue;
            }

            if (filter(currentStudent))
            {
                activeStudents.Add(currentStudent);
            }
        }

        return activeStudents.AsReadOnly();
    }

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
