//
//   RoleAssignmentInfo.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Kos.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;

/// <summary>
/// Information used to determine what roles a user should be assigned.
/// </summary>
/// <param name="Username">The username.</param>
/// <param name="Years">The years to assign roles for.</param>
/// <param name="ProgrammeTypes">The programme types to assign roles for.</param>
/// <param name="UsermapRoles">The usermap roles to assign.</param>
/// <param name="PreTitles">The pre-titles to assign roles for.</param>
/// <param name="PostTitles">The post-titles to assign roles for.</param>
/// <param name="ActiveProgrammes">The active programmes to assign roles for.</param>
/// <param name="GraduatedProgrammes">The graduated programmes to assign roles for.</param>
/// <param name="Teacher">Whether the user is a teacher.</param>
public record RoleAssignmentInfo(
    string Username,
    IEnumerable<int>? Years = default,
    IEnumerable<ProgrammeType>? ProgrammeTypes = default,
    IEnumerable<string>? UsermapRoles = default,
    IEnumerable<string>? PreTitles = default,
    IEnumerable<string>? PostTitles = default,
    IEnumerable<string>? ActiveProgrammes = default,
    IEnumerable<string>? GraduatedProgrammes = default,
    bool Teacher = false
);
