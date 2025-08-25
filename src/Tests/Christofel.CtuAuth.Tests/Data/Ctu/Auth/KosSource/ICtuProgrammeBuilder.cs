//
//   ICtuProgrammeBuilder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Kos.Data;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;

/// <summary>
/// Interface for building programme test data.
/// </summary>
public interface ICtuProgrammeBuilder
{
    /// <summary>
    /// Sets the programme type.
    /// </summary>
    /// <param name="type">The programme type.</param>
    /// <returns>This builder.</returns>
    ICtuProgrammeBuilder WithType(ProgrammeType type);

    /// <summary>
    /// Sets the programme name.
    /// </summary>
    /// <param name="name">The programme name.</param>
    /// <returns>This builder.</returns>
    ICtuProgrammeBuilder WithName(string name);

    /// <summary>
    /// Finishes building the programme and returns to the source builder.
    /// </summary>
    /// <returns>The source builder.</returns>
    ICtuSourceBuilder Finish();
}
