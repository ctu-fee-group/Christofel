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
    private readonly CoursesRepository _coursesRepository;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly CoursesContext _coursesContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesChannelUserAssigner"/> class.
    /// </summary>
    /// <param name="coursesRepository">The courses repository.</param>
    /// <param name="channelApi">The channel rest api.</param>
    /// <param name="coursesContext">The courses database context.</param>
    public CoursesChannelUserAssigner
    (
        CoursesRepository coursesRepository,
        IDiscordRestChannelAPI channelApi,
        CoursesContext coursesContext
    )
    {
        _coursesRepository = coursesRepository;
        _channelApi = channelApi;
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
                await AddCourseUsers(user, course, ct);

                var permissionsResult = await _channelApi.EditChannelPermissionsAsync
                (
                    course.ChannelId,
                    user.DiscordId,
                    allow: new DiscordPermissionSet(DiscordPermission.ViewChannel),
                    type: PermissionOverwriteType.Member,
                    reason: "Course assignment",
                    ct: ct
                );

                if (!permissionsResult.IsSuccess)
                {
                    return Result<bool>.FromError(permissionsResult);
                }

                return true;
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
    public async Task<CoursesAssignmentResult> DeassignCourses
        (IDiscordUser user, IEnumerable<string> courseKeys, CancellationToken ct = default)
        => await DoCoursesOperationAsync
        (
            courseKeys,
            async (course, ct) =>
            {
                await RemoveCourseUsers(user, course, ct);

                var permissionsResult = await _channelApi.EditChannelPermissionsAsync
                (
                    course.ChannelId,
                    user.DiscordId,
                    deny: new DiscordPermissionSet(DiscordPermission.ViewChannel),
                    type: PermissionOverwriteType.Member,
                    reason: "Course assignment",
                    ct: ct
                );

                if (!permissionsResult.IsSuccess)
                {
                    return Result<bool>.FromError(permissionsResult);
                }

                return false;
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
                    return Result<bool>.FromError(channelResult);
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

                if (hasViewPermission)
                {
                    await RemoveCourseUsers(user, course, ct);
                }
                else
                {
                    await AddCourseUsers(user, course, ct);
                }

                var permissionsResult = await _channelApi.EditChannelPermissionsAsync
                (
                    course.ChannelId,
                    user.DiscordId,
                    allow: allowSet,
                    deny: denySet,
                    type: PermissionOverwriteType.Member,
                    reason: "Course assignment",
                    ct: ct
                );

                if (!permissionsResult.IsSuccess)
                {
                    return Result<bool>.FromError(permissionsResult);
                }

                return !hasViewPermission;
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
    public async Task<Result<CoursesAssignmentResult>> AssignSemesterCourses
        (ILinkUser user, string semesterSelector, CancellationToken ct = default)
    {
        var semesterCoursesResult = await _coursesRepository.GetSemesterCoursesKeys(user, semesterSelector, ct);

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
    public async Task<Result<CoursesAssignmentResult>> DeassignSemesterCourses
        (ILinkUser user, string semesterSelector, CancellationToken ct = default)
    {
        var semesterCoursesResult = await _coursesRepository.GetSemesterCoursesKeys(user, semesterSelector, ct);

        if (!semesterCoursesResult.IsDefined(out var semesterCourses))
        {
            return Result<CoursesAssignmentResult>.FromError(semesterCoursesResult);
        }

        return await DeassignCourses(user, semesterCourses, ct);
    }

    private async Task AddCourseUsers(IDiscordUser user, CourseAssignment course, CancellationToken ct)
    {
        if (await _coursesContext.CourseUsers.AllAsync
            (x => x.CourseKey == course.CourseKey && x.UserDiscordId == user.DiscordId, ct))
        {
            _coursesContext.Add
            (
                new CourseUser
                {
                    CourseKey = course.CourseKey,
                    UserDiscordId = user.DiscordId
                }
            );
            await _coursesContext.SaveChangesAsync();
        }
    }

    private async Task RemoveCourseUsers(IDiscordUser user, CourseAssignment course, CancellationToken ct)
    {
        var courseUsers = await _coursesContext.CourseUsers
            .Where(x => x.CourseKey == course.CourseKey && x.UserDiscordId == user.DiscordId)
            .ToListAsync(ct);
        _coursesContext.RemoveRange(courseUsers);
        await _coursesContext.SaveChangesAsync();
    }

    private async Task<Result<CourseAssignment?>> GetCourseAssignment(string courseKey)
    {
        try
        {
            return await _coursesContext.Set<CourseAssignment>()
                .AsNoTracking()
                .Include(x => x.GroupAssignment)
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
        Func<CourseAssignment, CancellationToken, Task<Result<bool>>> operation,
        CancellationToken ct = default
    )
    {
        var errors = new Dictionary<string, Result>();
        var assigned = new List<CourseAssignment>();
        var deassigned = new List<CourseAssignment>();
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

            Result<bool> operationResult;
            if (assigned.Any(x => x.ChannelId == course.ChannelId))
            {
                operationResult = true;
            }
            else if (deassigned.Any(x => x.ChannelId == course.ChannelId))
            {
                operationResult = false;
            }
            else
            {
                try
                {
                    operationResult = await operation(course, ct);
                }
                catch (Exception e)
                {
                    operationResult = e;
                }
            }

            if (operationResult.IsDefined(out var operationAssigned))
            {
                if (operationAssigned)
                {
                    assigned.Add(course);
                }
                else
                {
                    deassigned.Add(course);
                }
            }
            else
            {
                errors.Add(courseKey, Result.FromError(operationResult));
            }
        }

        return new CoursesAssignmentResult(assigned, deassigned, missing, errors);
    }
}