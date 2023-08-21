//
//   CoursesChannelCreator.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.BaseLib.Configuration;
using Christofel.Common.Database;
using Christofel.CoursesLib.Database;
using Christofel.CoursesLib.Extensions;
using Kos.Abstractions;
using Kos.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Options;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Christofel.CoursesLib.Services;

/// <summary>
/// A class for creating channels for courses.
/// </summary>
public class CoursesChannelCreator
{
    private readonly CoursesContext _coursesContext;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IKosDivisionsApi _divisionsApi;
    private readonly IKosCoursesApi _coursesApi;
    private readonly BotOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesChannelCreator"/> class.
    /// </summary>
    /// <param name="coursesContext">The courses database context.</param>
    /// <param name="guildApi">The discord rest guild api.</param>
    /// <param name="channelApi">The discord rest channel api.</param>
    /// <param name="divisionsApi">The divisions api.</param>
    /// <param name="coursesApi">The kos courses api.</param>
    /// <param name="options">The bot options.</param>
    public CoursesChannelCreator
    (
        CoursesContext coursesContext,
        IDiscordRestGuildAPI guildApi,
        IDiscordRestChannelAPI channelApi,
        IKosDivisionsApi divisionsApi,
        IKosCoursesApi coursesApi,
        IOptions<BotOptions> options
    )
    {
        _coursesContext = coursesContext;
        _guildApi = guildApi;
        _channelApi = channelApi;
        _divisionsApi = divisionsApi;
        _coursesApi = coursesApi;
        _options = options.Value;
    }

    /// <summary>
    /// Creates a channel for the given course.
    /// </summary>
    /// <param name="courseKey">The key of the course to add.</param>
    /// <param name="channelName">The name of the channel. If null it will be guessed from the key of the course.</param>
    /// <param name="ct">The cancellation token for cancelling the operation.</param>
    /// <returns>A result which may or may not have succeeded.</returns>
    public async Task<Result> CreateCourseChannel
        (string courseKey, string? channelName = default, CancellationToken ct = default)
    {
        if (await _coursesContext.Set<CourseAssignment>()
            .AnyAsync(x => x.CourseKey == courseKey, ct))
        {
            return new InvalidOperationError("The given course is already linked to a channel.");
        }

        var course = await _coursesApi.GetCourse(courseKey, token: ct);

        if (course is null)
        {
            return new NotFoundError("Course with the given key was not found.");
        }
        var departmentKey = course.Department.GetKey();

        // 1. find LAST department category
        // 2. Try to add the course to the category
        // 3. If it cannot be added and it is due to the 50 channels limit, create a new category with the name "<department> X"
        // 4. Save the department to database
        // 5. Repeat 1.

        if (channelName is null)
        {
            var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            var courseKeyTrimmed = courseKey.TrimEnd(digits);

            channelName = courseKey[(courseKeyTrimmed.LastIndexOfAny(digits) + 1)..].ToLower();
        }

        var departmentAssignment = await _coursesContext.DepartmentAssignments
            .OrderBy(x => x.Id)
            .LastOrDefaultAsync(x => x.DepartmentKey == departmentKey, ct);

        if (departmentAssignment is null)
        {
            return new NotFoundError($"Could not find department {departmentKey} for course {courseKey}");
        }

        var channelResult = await _guildApi.CreateGuildChannelAsync
            (DiscordSnowflake.New(_options.GuildId), channelName, parentID: departmentAssignment.CategoryId, ct: ct);

        if (!channelResult.IsDefined(out var channel))
        {
            return Result.FromError(channelResult);
        }

        if (!await _coursesContext.CourseGroupAssignments.AnyAsync(x => x.ChannelId == channel.ID, ct))
        {
            var courseGroupAssignment = new CourseGroupAssignment
            {
                ChannelId = channel.ID,
                Name = course.Name
            };

            _coursesContext.Add(courseGroupAssignment);
        }

        var courseAssignment = new CourseAssignment
        {
            ChannelId = channel.ID,
            CourseKey = courseKey,
            CourseName = course.Name,
            ChannelName = channelName,
            DepartmentKey = course.Department.GetKey()
        };

        try
        {
            _coursesContext.Add(courseAssignment);
            await _coursesContext.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Updates a link of a course.
    /// </summary>
    /// <param name="courseKey">The key of the course.</param>
    /// <param name="courseChannelId">The channel id to link course to.</param>
    /// <param name="roleId">The id of the role that gives the channel.</param>
    /// <param name="ct">The cancellation token for cancelling the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public async Task<Result> UpdateCourseLink
    (
        string courseKey,
        Snowflake courseChannelId,
        Snowflake? roleId = default,
        CancellationToken ct = default
    )
    {
        var course = await _coursesContext.CourseAssignments
            .FirstOrDefaultAsync(x => x.CourseKey == courseKey, ct);

        if (course is null)
        {
            return new NotFoundError("The given course is not linked to a channel");
        }

        course.ChannelId = courseChannelId;
        course.RoleId = roleId;

        try
        {
            await _coursesContext.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            return new ExceptionError(e);
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Adds a link of a course to already existing channel.
    /// </summary>
    /// <param name="courseKey">The key of the course.</param>
    /// <param name="courseChannelId">The channel id to link course to.</param>
    /// <param name="roleId">The id of the role that gives the channel.</param>
    /// <param name="ct">The cancellation token for cancelling the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public async Task<Result> CreateCourseLink
    (
        string courseKey,
        Snowflake courseChannelId,
        Snowflake? roleId = default,
        CancellationToken ct = default
    )
    {
        if (await _coursesContext.Set<CourseAssignment>()
            .AnyAsync(x => x.CourseKey == courseKey, ct))
        {
            return new InvalidOperationError("The given course is already linked to a channel.");
        }

        var course = await _coursesApi.GetCourse(courseKey, token: ct);

        if (course is null)
        {
            return new NotFoundError("Course with the given key was not found.");
        }

        var channelResult = await _channelApi.GetChannelAsync(courseChannelId, ct);

        if (!channelResult.IsDefined(out var channel))
        {
            return Result.FromError(channelResult);
        }

        if (!await _coursesContext.CourseGroupAssignments.AnyAsync(x => x.ChannelId == courseChannelId, ct))
        {
            var courseGroupAssignment = new CourseGroupAssignment
            {
                ChannelId = courseChannelId,
                Name = course.Name
            };

            _coursesContext.Add(courseGroupAssignment);
        }

        var courseAssignment = new CourseAssignment
        {
            ChannelId = courseChannelId,
            RoleId = roleId,
            CourseKey = courseKey,
            CourseName = course.Name,
            ChannelName = channel.Name.HasValue ? channel.Name.Value : null,
            DepartmentKey = course.Department.GetKey()
        };

        try
        {
            _coursesContext.Add(courseAssignment);
            await _coursesContext.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Removes a link of course - channel from the database. The channel won't be removed.
    /// </summary>
    /// <param name="courseKey">The key of the course.</param>
    /// <param name="ct">The cancellation token for cancelling the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public async Task<Result> RemoveCourseLink(string courseKey, CancellationToken ct = default)
    {
        try
        {
            var courseAssignment = await _coursesContext.CourseAssignments.FirstOrDefaultAsync
                (x => x.CourseKey == courseKey, ct);

            if (courseAssignment is null)
            {
                return new NotFoundError("Course with the given key was not found.");
            }

            _coursesContext.Remove(courseAssignment);
            await _coursesContext.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Assigns the given category to the department.
    /// </summary>
    /// <param name="departmentKey">The key of the department that will be assigned.</param>
    /// <param name="categoryId">The id of the category to assign.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public async Task<Result> AssignDepartmentCategory
        (string departmentKey, Snowflake categoryId, CancellationToken ct = default)
    {
        var division = await _divisionsApi.GetDivision(departmentKey, token: ct);

        if (division is null)
        {
            return new NotFoundError($"Could not find the given division {departmentKey}.");
        }

        try
        {
            _coursesContext.Add
            (
                new DepartmentAssignment
                {
                    DepartmentKey = departmentKey,
                    DepartmentName = division.Name ?? departmentKey,
                    CategoryId = categoryId
                }
            );
            await _coursesContext.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }

    /// <summary>
    /// Deassigns the given category to the department.
    /// </summary>
    /// <param name="departmentKey">The key of the department that will be deassigned.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    public async Task<Result> DeassignDepartmentCategory(string departmentKey, CancellationToken ct = default)
    {
        try
        {
            var departmentAssignment = await _coursesContext.DepartmentAssignments
                .OrderBy(x => x.Id)
                .LastOrDefaultAsync(x => x.DepartmentKey == departmentKey, ct);

            if (departmentAssignment is not null)
            {
                _coursesContext.Remove(departmentAssignment);
                await _coursesContext.SaveChangesAsync(ct);
            }
        }
        catch (Exception e)
        {
            return e;
        }

        return Result.FromSuccess();
    }
}