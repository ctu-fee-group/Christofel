//
//   CtuAuthProcessRolesTests.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;
using Christofel.CtuAuth.Tests.Runners;
using Kos.Data;

namespace Christofel.CtuAuth.Tests.Ctu.Auth;

/// <summary>
/// Tests for CtuAuthProcess that are looking for correct roles
/// being assigned to users.
/// </summary>
public class CtuAuthProcessRolesTests
{
#pragma warning disable SA1010

    /// <summary>
    /// Tests a regular bachelor's user from FEE that is studying bachelor.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
    [Fact]
    public async Task RegularBachelorUserHasAllRoles()
        => await CtuAuthTestRunner.BuildAndTestSteps(
            "user1",
            builder => builder
                    .AddFaculty("13000")
                      .WithName("Faculty of electrical engineering")
                      .WithAbbreviation("FEE")
                    .Finish()
                    .AddProgramme("EK-B")
                      .WithName("Programme1")
                      .WithType(ProgrammeType.Bachelor)
                    .Finish()
                    .AddPerson("user1")
                      .WithUsermapRole("A_USERMAP_ROLE")
                      .AddStudentRole()
                        .WithStartDate(2020, 06, 30)
                        .WithFaculty("13000")
                        .WithProgramme("EK-B")
                        .Studying()
                      .Finish()
                    .Finish(),
            new RoleAssignmentInfo
            (
                "user1",
                Years: [2020],
                ProgrammeTypes: [ProgrammeType.Bachelor],
                UsermapRoles: ["A_USERMAP_ROLE"],
                ActiveProgrammes: ["Programme1"]
            )
        );

    /// <summary>
    /// Tests a regular master's user from FEE that is studying master's.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
    [Fact]
    public async Task RegularMasterUserHasAllRoles()
        => await CtuAuthTestRunner.BuildAndTestSteps(
            "user1",
            builder => builder
                    .AddFaculty("13000")
                      .WithName("Faculty of electrical engineering")
                      .WithAbbreviation("FEE")
                    .Finish()
                    .AddProgramme("EK-B")
                      .WithName("Programme1")
                      .WithType(ProgrammeType.Bachelor)
                    .Finish()
                    .AddProgramme("EK-M")
                      .WithName("Programme1")
                      .WithType(ProgrammeType.Master)
                    .Finish()
                    .AddPerson("user1")
                      .WithUsermapRole("ANOTHER_USERMAP_ROLE")
                      .AddStudentRole()
                        .WithStartDate(2020, 06, 30)
                        .WithEndDate(2024, 06, 15)
                        .WithFaculty("13000")
                        .WithProgramme("EK-B")
                        .Graduated()
                      .Finish()
                      .AddStudentRole()
                        .WithStartDate(2024, 06, 30)
                        .WithFaculty("13000")
                        .WithProgramme("EK-M")
                        .Studying()
                      .Finish()
                    .Finish(),
            new RoleAssignmentInfo
            (
                "user1",
                Years: [2020],
                ProgrammeTypes: [ProgrammeType.Master],
                UsermapRoles: ["ANOTHER_USERMAP_ROLE"],
                ActiveProgrammes: ["Programme1"],
                GraduatedProgrammes: []
            )
        );

    /// <summary>
    /// Tests a regular master's user from FEE that is studying master, going from a different programme from bachelor's.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
    [Fact]
    public async Task MasterTransferUserHasAllRoles()
        => await CtuAuthTestRunner.BuildAndTestSteps(
            "user1",
            builder => builder
                    .AddFaculty("13000")
                      .WithName("Faculty of electrical engineering")
                      .WithAbbreviation("FEE")
                    .Finish()
                    .AddProgramme("EK-B")
                      .WithName("Programme1")
                      .WithType(ProgrammeType.Bachelor)
                    .Finish()
                    .AddProgramme("EK-M")
                      .WithName("Programme1")
                      .WithType(ProgrammeType.Master)
                    .Finish()
                    .AddProgramme("BIO-M")
                      .WithName("Programme2")
                      .WithType(ProgrammeType.Master)
                    .Finish()
                    .AddPerson("user1")
                      .WithUsermapRole("ANOTHER_USERMAP_ROLE")
                      .WithUsermapRole("A_USERMAP_ROLE")
                      .AddStudentRole()
                        .WithStartDate(2020, 06, 30)
                        .WithEndDate(2024, 06, 15)
                        .WithFaculty("13000")
                        .WithProgramme("EK-B")
                        .Graduated()
                      .Finish()
                      .AddStudentRole()
                        .WithStartDate(2024, 06, 30)
                        .WithFaculty("13000")
                        .WithProgramme("BIO-M")
                        .Studying()
                      .Finish()
                    .Finish(),
            new RoleAssignmentInfo
            (
                "user1",
                Years: [2020],
                ProgrammeTypes: [ProgrammeType.Master],
                UsermapRoles: ["ANOTHER_USERMAP_ROLE", "A_USERMAP_ROLE"],
                ActiveProgrammes: ["Programme2"],
                GraduatedProgrammes: ["Programme1"]
            )
        );

    /// <summary>
    /// Tests a regular master's user from FEE that is studying master, going from a different programme from bachelor's.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
    [Fact]
    public async Task BachelorTransferUserHasAllRoles()
        => await CtuAuthTestRunner.BuildAndTestSteps(
            "anotherusername",
            builder => builder
                    .AddFaculty("13000")
                      .WithName("Faculty of electrical engineering")
                      .WithAbbreviation("FEE")
                    .Finish()
                    .AddProgramme("EK-B")
                      .WithName("Programme1")
                      .WithType(ProgrammeType.Bachelor)
                    .Finish()
                    .AddProgramme("BIO-B")
                      .WithName("Programme2")
                      .WithType(ProgrammeType.Bachelor)
                    .Finish()
                    .AddPerson("anotherusername")
                      .WithUsermapRole("ANOTHER_USERMAP_ROLE")
                      .WithUsermapRole("A_USERMAP_ROLE")
                      .AddStudentRole()
                        .WithStartDate(2020, 06, 30)
                        .WithEndDate(2024, 06, 15)
                        .WithFaculty("13000")
                        .WithProgramme("EK-B")
                        .Withdrew()
                      .Finish()
                      .AddStudentRole()
                        .WithStartDate(2024, 06, 30)
                        .WithFaculty("13000")
                        .WithProgramme("BIO-B")
                        .Studying()
                      .Finish()
                    .Finish(),
            new RoleAssignmentInfo
            (
                "anotherusername",
                Years: [2020],
                ProgrammeTypes: [ProgrammeType.Bachelor],
                UsermapRoles: ["ANOTHER_USERMAP_ROLE", "A_USERMAP_ROLE"],
                ActiveProgrammes: ["Programme2"]
            )
        );

    /// <summary>
    /// Student continued same programme on master, but then switched to another.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
    [Fact]
    public async Task MasterTransfer2UserHasAllRoles()
        => await CtuAuthTestRunner.BuildAndTestSteps(
            "anotherusername",
            builder => builder
                    .AddFaculty("13000")
                      .WithName("Faculty of electrical engineering")
                      .WithAbbreviation("FEE")
                    .Finish()
                    .AddProgramme("EK-B")
                      .WithName("Programme1")
                      .WithType(ProgrammeType.Bachelor)
                    .Finish()
                    .AddProgramme("BIO-B")
                      .WithName("Programme2")
                      .WithType(ProgrammeType.Bachelor)
                    .Finish()
                    .AddProgramme("EK-M")
                      .WithName("Programme1")
                      .WithType(ProgrammeType.Master)
                    .Finish()
                    .AddProgramme("BIO-M")
                      .WithName("Programme2")
                      .WithType(ProgrammeType.Master)
                    .Finish()
                    .AddPerson("anotherusername")
                      .WithUsermapRole("ANOTHER_USERMAP_ROLE")
                      .WithUsermapRole("A_USERMAP_ROLE")
                      .AddStudentRole()
                        .WithStartDate(2020, 06, 30)
                        .WithEndDate(2024, 06, 15)
                        .WithFaculty("13000")
                        .WithProgramme("BIO-B")
                        .Graduated()
                      .Finish()
                      .AddStudentRole()
                        .WithStartDate(2024, 06, 30)
                        .WithFaculty("13000")
                        .WithProgramme("BIO-M")
                        .Withdrew()
                      .Finish()
                      .AddStudentRole()
                        .WithStartDate(2025, 06, 30)
                        .WithFaculty("13000")
                        .WithProgramme("EK-M")
                        .Studying()
                      .Finish()
                    .Finish(),
            new RoleAssignmentInfo
            (
                "anotherusername",
                Years: [2020],
                ProgrammeTypes: [ProgrammeType.Master],
                UsermapRoles: ["ANOTHER_USERMAP_ROLE", "A_USERMAP_ROLE"],
                ActiveProgrammes: ["Programme1"],
                GraduatedProgrammes: ["Programme2"]
            )
        );

    /// <summary>
    /// Student continued same programme on master, but then switched to another.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
    [Fact]
    public async Task UserHasMultipleGraduatedRoles()
        => await CtuAuthTestRunner.BuildAndTestSteps(
            "anotherusername",
            builder => builder
                    .AddFaculty("13000")
                      .WithName("Faculty of electrical engineering")
                      .WithAbbreviation("FEE")
                    .Finish()
                    .AddProgramme("EK-B")
                      .WithName("Programme1")
                      .WithType(ProgrammeType.Bachelor)
                    .Finish()
                    .AddProgramme("BIO-B")
                      .WithName("Programme2")
                      .WithType(ProgrammeType.Bachelor)
                    .Finish()
                    .AddProgramme("EK-M")
                      .WithName("Programme1")
                      .WithType(ProgrammeType.Master)
                    .Finish()
                    .AddProgramme("BIO-M")
                      .WithName("Programme2")
                      .WithType(ProgrammeType.Master)
                    .Finish()
                    .AddPerson("anotherusername")
                      .WithUsermapRole("ANOTHER_USERMAP_ROLE")
                      .WithUsermapRole("A_USERMAP_ROLE")
                      .AddStudentRole()
                        .WithStartDate(2020, 06, 30)
                        .WithEndDate(2024, 06, 15)
                        .WithFaculty("13000")
                        .WithProgramme("BIO-B")
                        .Graduated()
                      .Finish()
                      .AddStudentRole()
                        .WithStartDate(2020, 06, 30)
                        .WithEndDate(2024, 06, 15)
                        .WithFaculty("13000")
                        .WithProgramme("EK-B")
                        .Graduated()
                      .Finish()
                      .AddStudentRole()
                        .WithStartDate(2025, 06, 30)
                        .WithFaculty("13000")
                        .WithProgramme("EK-M")
                        .Studying()
                      .Finish()
                      .AddTeacherRole()
                      .Finish()
                    .Finish(),
            new RoleAssignmentInfo
            (
                "anotherusername",
                Years: [2020],
                ProgrammeTypes: [ProgrammeType.Master],
                UsermapRoles: ["ANOTHER_USERMAP_ROLE", "A_USERMAP_ROLE"],
                ActiveProgrammes: ["Programme1"],
                GraduatedProgrammes: ["Programme2"], // Programme1 not in graduated roles as still active
                Teacher: true
            )
        );

    /// <summary>
    /// Student continued same programme on master, but then switched to another.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
    [Fact]
    public async Task UserHasMultipleActiveProgrammes()
        => await CtuAuthTestRunner.BuildAndTestSteps(
            "anotherusername",
            builder => builder
                    .AddFaculty("13000")
                      .WithName("Faculty of electrical engineering")
                      .WithAbbreviation("FEE")
                    .Finish()
                    .AddProgramme("EK-B")
                      .WithName("Programme1")
                      .WithType(ProgrammeType.Bachelor)
                    .Finish()
                    .AddProgramme("BIO-B")
                      .WithName("Programme2")
                      .WithType(ProgrammeType.Bachelor)
                    .Finish()
                    .AddProgramme("OI-M")
                      .WithName("Programme3")
                      .WithType(ProgrammeType.Master)
                    .Finish()
                    .AddProgramme("EEM-M")
                      .WithName("Programme4")
                      .WithType(ProgrammeType.Master)
                    .Finish()
                    .AddPerson("anotherusername")
                      .WithUsermapRole("ANOTHER_USERMAP_ROLE")
                      .WithUsermapRole("A_USERMAP_ROLE")
                      .AddStudentRole()
                        .WithStartDate(2020, 06, 30)
                        .WithEndDate(2024, 06, 15)
                        .WithFaculty("13000")
                        .WithProgramme("BIO-B")
                        .Graduated()
                      .Finish()
                      .AddStudentRole()
                        .WithStartDate(2020, 06, 30)
                        .WithEndDate(2024, 06, 15)
                        .WithFaculty("13000")
                        .WithProgramme("EK-B")
                        .Graduated()
                      .Finish()
                      .AddStudentRole()
                        .WithStartDate(2025, 06, 30)
                        .WithFaculty("13000")
                        .WithProgramme("EEM-M")
                        .Studying()
                      .Finish()
                      .AddStudentRole()
                        .WithStartDate(2025, 06, 30)
                        .WithFaculty("13000")
                        .WithProgramme("OI-M")
                        .Studying()
                      .Finish()
                    .Finish(),
            new RoleAssignmentInfo
            (
                "anotherusername",
                Years: [2020],
                ProgrammeTypes: [ProgrammeType.Master],
                UsermapRoles: ["ANOTHER_USERMAP_ROLE", "A_USERMAP_ROLE"],
                ActiveProgrammes: ["Programme3", "Programme4"],
                GraduatedProgrammes: ["Programme1", "Programme2"]
            )
        );

    /// <summary>
    /// Tests that if the user has multiple titles, the one with most priority is assigned.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
    [Fact]
    public async Task MostPriorityTitleRoleIsAssigned()
        => await CtuAuthTestRunner.BuildAndTestSteps(
            "user1",
            builder => builder
                    .AddPerson("user1")
                      .WithName("Tester Testing")
                      .DoNotIncludeInKosApi()
                      .WithPreTitles(["Bc.", "Mgr."])
                      .WithPostTitles(["Ph.D."])
                    .Finish(),
            new RoleAssignmentInfo
            (
                "user1",
                PreTitles: ["Mgr."],
                PostTitles: ["Ph.D."]
            )
        );

    /// <summary>
    /// Tests that if the user has multiple titles of the same priority, all are assigned.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
    [Fact]
    public async Task MultipleMostPriorityTitlesRolesAreAssigned()
        => await CtuAuthTestRunner.BuildAndTestSteps(
            "user1",
            builder => builder
                    .AddPerson("user1")
                      .WithName("Tester Testing")
                      .DoNotIncludeInKosApi()
                      .WithPreTitles(["Bc.", "Ing.", "Mgr."])
                      .WithPostTitles(["Ph.D."])
                    .Finish(),
            new RoleAssignmentInfo
            (
                "user1",
                PreTitles: ["Mgr.", "Ing."],
                PostTitles: ["Ph.D."]
            )
        );

    /// <summary>
    /// Tests usermap fallback for titles assignment.
    /// Titles should be taken from UsermapAPI if the person doesn't have kosapi record.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operations.</returns>
    [Fact]
    public async Task TitlesComeFromUsermapFallback()
        => await CtuAuthTestRunner.BuildAndTestSteps(
            "user1",
            builder => builder
                    .AddPerson("user1")
                      .WithName("Tester Testing")
                      .DoNotIncludeInKosApi()
                      .WithPreTitles(["Bc."])
                      .WithPostTitles(["Ph.D."])
                      .WithUsermapRole("A_USERMAP_ROLE")
                    .Finish(),
            new RoleAssignmentInfo
            (
                "user1",
                UsermapRoles: ["A_USERMAP_ROLE"],
                PreTitles: ["Bc."],
                PostTitles: ["Ph.D."]
            )
        );
}
