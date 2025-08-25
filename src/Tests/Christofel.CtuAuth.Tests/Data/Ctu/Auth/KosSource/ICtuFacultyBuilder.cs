//
//   ICtuFacultyBuilder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;

/// <summary>
/// Interface for building faculty test data.
/// </summary>
public interface ICtuFacultyBuilder
{
    /// <summary>
    /// Sets the faculty name.
    /// </summary>
    /// <param name="name">The faculty name.</param>
    /// <returns>This builder.</returns>
    ICtuFacultyBuilder WithName(string name);

    /// <summary>
    /// Sets the faculty abbreviation.
    /// </summary>
    /// <param name="abbreviation">The faculty abbreviation.</param>
    /// <returns>This builder.</returns>
    ICtuFacultyBuilder WithAbbreviation(string abbreviation);

    /// <summary>
    /// Finishes building the faculty and returns to the source builder.
    /// </summary>
    /// <returns>The source builder.</returns>
    ICtuSourceBuilder Finish();
}