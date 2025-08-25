//
//   ICtuSourceBuilder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Usermap.Data;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;

/// <summary>
/// Interface for building CTU test data sources.
/// </summary>
public interface ICtuSourceBuilder
{
    /// <summary>
    /// Adds a faculty with the specified code.
    /// </summary>
    /// <param name="code">The faculty code.</param>
    /// <returns>A faculty builder.</returns>
    ICtuFacultyBuilder AddFaculty(string code);

    /// <summary>
    /// Adds a programme with the specified code.
    /// </summary>
    /// <param name="code">The programme code.</param>
    /// <returns>A programme builder.</returns>
    ICtuProgrammeBuilder AddProgramme(string code);

    /// <summary>
    /// Adds a person with the specified username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>A user builder.</returns>
    ICtuUserBuilder AddPerson(string username);

    /// <summary>
    /// Builds the KOS API source from the configured data.
    /// </summary>
    /// <returns>A KOS API source containing all the built data.</returns>
    KosApiSource BuildKosApiSource();

    /// <summary>
    /// Builds the Usermap API source from the configured data.
    /// </summary>
    /// <returns>A dictionary of usernames to usermap persons.</returns>
    Dictionary<string, UsermapPerson> BuildUsermapApiSource();
}
