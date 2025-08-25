//
//   ICtuStudentRoleBuilder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Kos.Data;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;

/// <summary>
/// Interface for building student role test data.
/// </summary>
public interface ICtuStudentRoleBuilder
{
    /// <summary>
    /// Sets the faculty for the student role.
    /// </summary>
    /// <param name="facultyCode">The faculty code.</param>
    /// <returns>This builder.</returns>
    ICtuStudentRoleBuilder WithFaculty(string facultyCode);

    /// <summary>
    /// Sets the programme for the student role.
    /// </summary>
    /// <param name="programmeCode">The programme code.</param>
    /// <returns>This builder.</returns>
    ICtuStudentRoleBuilder WithProgramme(string programmeCode);

    /// <summary>
    /// Sets the start date for the student role.
    /// </summary>
    /// <param name="year">The start year.</param>
    /// <param name="month">The start month.</param>
    /// <param name="day">The start day.</param>
    /// <returns>This builder.</returns>
    ICtuStudentRoleBuilder WithStartDate(int year, int month, int day);

    /// <summary>
    /// Sets the end date for the student role.
    /// </summary>
    /// <param name="year">The end year.</param>
    /// <param name="month">The end month.</param>
    /// <param name="day">The end day.</param>
    /// <returns>This builder.</returns>
    ICtuStudentRoleBuilder WithEndDate(int year, int month, int day);

    /// <summary>
    /// Sets the study grade for the student role.
    /// </summary>
    /// <param name="grade">The study grade.</param>
    /// <returns>This builder.</returns>
    ICtuStudentRoleBuilder WithGrade(ushort grade);

    /// <summary>
    /// Sets the student as graduated.
    /// </summary>
    /// <returns>This builder.</returns>
    ICtuStudentRoleBuilder Graduated();

    /// <summary>
    /// Sets the student as currently studying.
    /// </summary>
    /// <returns>This builder.</returns>
    ICtuStudentRoleBuilder Studying();

    /// <summary>
    /// Sets the student as withdrew.
    /// </summary>
    /// <returns>This builder.</returns>
    ICtuStudentRoleBuilder Withdrew();

    /// <summary>
    /// Finishes building the student role and returns to the user builder.
    /// </summary>
    /// <returns>The user builder.</returns>
    ICtuUserBuilder Finish();
}
