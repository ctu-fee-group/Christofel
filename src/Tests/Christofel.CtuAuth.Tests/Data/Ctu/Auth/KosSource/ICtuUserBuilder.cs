//
//   ICtuUserBuilder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;

/// <summary>
/// Interface for building user test data.
/// </summary>
public interface ICtuUserBuilder
{
    /// <summary>
    /// Sets the user's pre-titles.
    /// </summary>
    /// <param name="titles">The pre-titles.</param>
    /// <returns>This builder.</returns>
    ICtuUserBuilder WithPreTitles(string[] titles);

    /// <summary>
    /// Sets the user's post-titles.
    /// </summary>
    /// <param name="titles">The post-titles.</param>
    /// <returns>This builder.</returns>
    ICtuUserBuilder WithPostTitles(string[] titles);

    /// <summary>
    /// The person shouldn't be returned by kosapi.
    /// </summary>
    /// <remarks>
    /// This is useful for testing Usermap fallback.
    /// </remarks>
    /// <returns>This builder.</returns>
    ICtuUserBuilder DoNotIncludeInKosApi();

    /// <summary>
    /// Adds a usermap role to the user.
    /// </summary>
    /// <param name="role">The usermap role.</param>
    /// <returns>This builder.</returns>
    ICtuUserBuilder WithUsermapRole(string role);

    /// <summary>
    /// Sets the user's name.
    /// </summary>
    /// <param name="name">The user's name.</param>
    /// <returns>This builder.</returns>
    ICtuUserBuilder WithName(string name);

    /// <summary>
    /// Adds a student role to the user.
    /// </summary>
    /// <returns>A student role builder.</returns>
    ICtuStudentRoleBuilder AddStudentRole();

    /// <summary>
    /// Adds a teacher role to the user.
    /// </summary>
    /// <returns>A teacher role builder.</returns>
    ICtuTeacherRoleBuilder AddTeacherRole();

    /// <summary>
    /// Finishes building the user and returns to the source builder.
    /// </summary>
    /// <returns>The source builder.</returns>
    ICtuSourceBuilder Finish();
}
