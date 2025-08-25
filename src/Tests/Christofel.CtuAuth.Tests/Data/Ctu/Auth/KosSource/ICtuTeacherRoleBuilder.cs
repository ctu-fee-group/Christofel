//
//   ICtuTeacherRoleBuilder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;

/// <summary>
/// Interface for building teacher role test data.
/// </summary>
public interface ICtuTeacherRoleBuilder
{
    /// <summary>
    /// Sets the division for the teacher role.
    /// </summary>
    /// <param name="divisionCode">The division code.</param>
    /// <returns>This builder.</returns>
    ICtuTeacherRoleBuilder WithDivision(string divisionCode);

    /// <summary>
    /// Sets whether the teacher is external.
    /// </summary>
    /// <param name="external">True if external, false otherwise.</param>
    /// <returns>This builder.</returns>
    ICtuTeacherRoleBuilder WithExternal(bool external);

    /// <summary>
    /// Sets the email for the teacher.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>This builder.</returns>
    ICtuTeacherRoleBuilder WithEmail(string email);

    /// <summary>
    /// Sets the phone for the teacher.
    /// </summary>
    /// <param name="phone">The phone number.</param>
    /// <returns>This builder.</returns>
    ICtuTeacherRoleBuilder WithPhone(string phone);

    /// <summary>
    /// Sets a random start date for the teacher role.
    /// </summary>
    /// <returns>This builder.</returns>
    ICtuTeacherRoleBuilder WithRandomStartDate();

    /// <summary>
    /// Finishes building the teacher role and returns to the user builder.
    /// </summary>
    /// <returns>The user builder.</returns>
    ICtuUserBuilder Finish();
}