//
//  CoursesChannelUserAssigner.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Common.Database;
using Christofel.Common.User;
using Christofel.CoursesLib.Data;
using Christofel.CoursesLib.Database;
using Christofel.CoursesLib.Extensions;
using Kos.Abstractions;
using Kos.Data;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.CoursesLib.Services;

/// <summary>
/// Assigns or deassigns course channels from Discord users.
/// </summary>
public class CoursesChannelUserAssigner
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IKosStudentsApi _studentsApi;
    private readonly IReadableDbContext<CoursesContext> _coursesContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesChannelUserAssigner"/> class.
    /// </summary>
    /// <param name="channelApi">The channel rest api.</param>
    /// <param name="studentsApi">The kos students api.</param>
    /// <param name="coursesContext">The courses database context.</param>
    public CoursesChannelUserAssigner
    (
        IDiscordRestChannelAPI channelApi,
        IKosStudentsApi studentsApi,
        IReadableDbContext<CoursesContext> coursesContext
    )
    {
        _channelApi = channelApi;
        _studentsApi = studentsApi;
        _coursesContext = coursesContext;
    }

    /// <summary>
    /// Tries to assign given courses to the given user.
    /// </summary>
    /// <param name="user">The user to assign courses to.</param>
    /// <param name="courseKeys">The keys of the courses to assign.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>Information about what courses have been added, what have not been found and what errors.</returns>
    public async Task<CoursesAssignmentResult> AssignCourses
        (IDiscordUser user, IEnumerable<string> courseKeys, CancellationToken ct = default)
        => await DoCoursesOperationAsync
        (
            courseKeys,
            async (course, ct) =>
            {
                return await _channelApi.EditChannelPermissionsAsync
                (
                    course.ChannelId,
                    user.DiscordId,
                    allow: new DiscordPermissionSet(DiscordPermission.ViewChannel),
                    type: PermissionOverwriteType.Member,
                    reason: "Course assignment",
                    ct: ct
                );
            },
            ct
        );

    /// <summary>
    /// Tries to deassign given courses to the given user.
    /// </summary>
    /// <param name="user">The user to assign courses to.</param>
    /// <param name="courseKeys">The keys of the courses to assign.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>Information about what courses have been added, what have not been found and what errors.</returns>
    public async Task<Result<CoursesAssignmentResult>> DeassignCourses
        (IDiscordUser user, IEnumerable<string> courseKeys, CancellationToken ct = default)
        => await DoCoursesOperationAsync
        (
            courseKeys,
            async (course, ct) =>
            {
                return await _channelApi.EditChannelPermissionsAsync
                (
                    course.ChannelId,
                    user.DiscordId,
                    deny: new DiscordPermissionSet(DiscordPermission.ViewChannel),
                    type: PermissionOverwriteType.Member,
                    reason: "Course assignment",
                    ct: ct
                );
            },
            ct
        );

    /// <summary>
    /// Tries to toggle (either assigns if the user does not have the course or deassignes if he has it) given courses to the given user.
    /// </summary>
    /// <param name="user">The user to assign courses to.</param>
    /// <param name="courseKeys">The keys of the courses to assign.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>Information about what courses have been added, what have not been found and what errors.</returns>
    public async Task<CoursesAssignmentResult> ToggleCourses
        (IDiscordUser user, IEnumerable<string> courseKeys, CancellationToken ct = default)
        => await DoCoursesOperationAsync
        (
            courseKeys,
            async (course, ct) =>
            {
                var channelResult = await _channelApi.GetChannelAsync(course.ChannelId, ct);

                if (!channelResult.IsDefined(out var channel))
                {
                    return Result.FromError(channelResult);
                }

                var hasViewPermission = false;

                // treat hasViewPermission as false is permission overwrites is not defined.
                if (channel.PermissionOverwrites.IsDefined(out var permissions))
                {
                    hasViewPermission = permissions.FirstOrDefault(x => x.ID == user.DiscordId)?.Allow.HasPermission
                        (DiscordPermission.ViewChannel) ?? false;
                }

                var allowSet = new DiscordPermissionSet(!hasViewPermission ? DiscordPermission.ViewChannel : default);
                var denySet = new DiscordPermissionSet(hasViewPermission ? DiscordPermission.ViewChannel : default);

                return await _channelApi.EditChannelPermissionsAsync
                (
                    course.ChannelId,
                    user.DiscordId,
                    allow: allowSet,
                    deny: denySet,
                    type: PermissionOverwriteType.Member,
                    reason: "Course assignment",
                    ct: ct
                );
            },
            ct
        );

    /// <summary>
    /// Assigns course channels based on semester.
    /// </summary>
    /// <param name="user">The user to assign courses to.</param>
    /// <param name="semesterSelector">The selector of the semester to assign courses from.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>Information about what courses have been added, what have not been found and what errors.</returns>
    public async Task<Result<CoursesAssignmentResult>> AssignSemesterCourses(ILinkUser user, string semesterSelector, CancellationToken ct = default)
    {
        var semesterCoursesResult = await GetSemesterCourses(user, semesterSelector, ct);

        if (!semesterCoursesResult.IsDefined(out var semesterCourses))
        {
            return Result<CoursesAssignmentResult>.FromError(semesterCoursesResult);
        }

        return await AssignCourses(user, semesterCourses, ct);
    }

    /// <summary>
    /// Deassigns course channels based on semester.
    /// </summary>
    /// <param name="user">The user to assign courses to.</param>
    /// <param name="semesterSelector">The selector of the semester to assign courses from.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>Information about what courses have been added, what have not been found and what errors.</returns>
    public async Task<Result<CoursesAssignmentResult>> DeassignSemesterCourses(ILinkUser user, string semesterSelector, CancellationToken ct = default)
    {
        var semesterCoursesResult = await GetSemesterCourses(user, semesterSelector, ct);

        if (!semesterCoursesResult.IsDefined(out var semesterCourses))
        {
            return Result<CoursesAssignmentResult>.FromError(semesterCoursesResult);
        }

        return await DeassignCourses(user, semesterCourses, ct);
    }

    private async Task<Result<IEnumerable<string>>> GetSemesterCourses(ILinkUser user, string semesterSelector, CancellationToken ct)
    {
        try
        {
            var enrolledCourses = await _studentsApi.GetStudentEnrolledCourses
                (user.CtuUsername, semesterSelector, limit: 100, token: ct);
            return Result<IEnumerable<string>>.FromSuccess(enrolledCourses
                .OfType<InternalCourseEnrollment>()
                .Where(x => x.Course is not null)
                .Select(x => x.Course!.GetKey()));
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private async Task<Result<CourseAssignment?>> GetCourseAssignment(string courseKey)
    {
        try
        {
            return await _coursesContext.Set<CourseAssignment>()
                .FirstOrDefaultAsync(x => x.CourseKey == courseKey);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private async Task<CoursesAssignmentResult> DoCoursesOperationAsync
    (
        IEnumerable<string> courseKeys,
        Func<CourseAssignment, CancellationToken, Task<Result>> operation,
        CancellationToken ct = default
    )
    {
        var errors = new Dictionary<string, Result>();
        var successful = new List<CourseAssignment>();
        var missing = new List<string>();

        foreach (var courseKey in courseKeys)
        {
            var courseResult = await GetCourseAssignment(courseKey);

            if (!courseResult.IsDefined(out var course))
            {
                if (!courseResult.IsSuccess)
                {
                    errors.Add(courseKey, Result.FromError(courseResult));
                }
                else
                {
                    missing.Add(courseKey);
                }

                continue;
            }

            var operationResult = await operation(course, ct);

            if (operationResult.IsSuccess)
            {
                successful.Add(course);
            }
            else
            {
                errors.Add(courseKey, operationResult);
            }
        }

        return new CoursesAssignmentResult(successful, missing, errors);
    }
}