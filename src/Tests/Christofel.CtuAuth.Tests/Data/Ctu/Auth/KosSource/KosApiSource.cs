//
//   KosApiSource.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Kos.Data;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;

/// <summary>
/// Represents a KOS API data source for testing.
/// </summary>
/// <param name="Faculties">The faculties (divisions) keyed by code.</param>
/// <param name="Programmes">The programmes keyed by code.</param>
/// <param name="Persons">The persons keyed by username.</param>
/// <param name="Students">The students keyed by ID.</param>
/// <param name="Teachers">The teachers keyed by ID.</param>
public record KosApiSource
(
    IReadOnlyDictionary<string, Division> Faculties,
    IReadOnlyDictionary<string, Programme> Programmes,
    IReadOnlyDictionary<string, Person> Persons,
    IReadOnlyDictionary<int, Student> Students,
    IReadOnlyDictionary<int, Teacher> Teachers
);
