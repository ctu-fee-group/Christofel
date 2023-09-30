//
//   CoursesAdminCommands.Search.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Christofel.CommandsLib.Validator;
using Christofel.CoursesLib.Extensions;
using FluentValidation;
using Kos.Abstractions;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Christofel.Courses.Commands;

/// <summary>
/// A class for /coursesadmin command group.
/// </summary>
public partial class CoursesAdminCommands
{
    /// <summary>
    /// A command group for /courses link.
    /// </summary>
    [Group("search")]
    public class SearchCommands : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly IKosCoursesApi _kosCourseApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCommands"/> class.
        /// </summary>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="kosCourseApi">The courses kos api.</param>
        public SearchCommands(FeedbackService feedbackService, IKosCoursesApi kosCourseApi)
        {
            _feedbackService = feedbackService;
            _kosCourseApi = kosCourseApi;
        }

        /// <summary>
        /// Searches for a single course by the given criteria. The key and name is anded.
        /// Both key and name may be only substrings of the course key or name, for the course to be found.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="name">The course name to search for.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("single")]
        public async Task<IResult> HandleSingleAsync(string? key = null, string? name = null)
            => await HandleMultipleAsync(key, name, limit: 1);

        /// <summary>
        /// Searches for multiple course by the given criteria. The key and name is joined by and.
        /// Both key and name may be only substrings of the course key or name, for the course to be found.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="name">The course name to search for.</param>
        /// <param name="limit">The limit to specify, to limit bandwidth. Default is 5, maximum 1000.</param>
        /// <returns>A result that may or may not have succeeded.</returns>
        [Command("multiple")]
        public async Task<IResult> HandleMultipleAsync(string? key = null, string? name = null, int limit = 5)
        {
            // Remove diacritics from name
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(name ?? string.Empty);
            var unaccentedName = System.Text.Encoding.UTF8.GetString(tempBytes);

            var allowedRegex = @"^[ a-zA-Z0-9]*$";
            var validationResult = new CommandValidator()
                .MakeSure
                (
                    "name",
                    unaccentedName ?? string.Empty,
                    o => o
                        .Matches(allowedRegex)
                        .WithMessage("Only alphabet, digits and spaces allowed")
                )
                .MakeSure
                (
                    "key",
                    key ?? string.Empty,
                    o => o
                        .Matches(allowedRegex)
                        .WithMessage("Only alphabet, digits and spaces allowed")
                )
                .MakeSure
                (
                    "name, key",
                    (name, key),
                    o => o
                        .Must((tuple, valueTuple) => tuple.key is not null || tuple.name is not null)
                        .WithMessage((_, _) => "Either name of key has to be specified.")
                )
                .MakeSure("limit", limit, o => o.InclusiveBetween(1, 1000))
                .Validate()
                .GetResult();

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var query = string.Empty;

            if (key is not null)
            {
                query += $"code=='*{key}*'";
            }

            if (key is not null && name is not null)
            {
                query += " and ";
            }

            if (name is not null)
            {
                query += $"name=='*{name}*'";
            }

            var courses = await _kosCourseApi.GetCourses
            (
                query: query,
                limit: (ushort)limit,
                token: CancellationToken
            );

            if (courses.Count == 0)
            {
                return await _feedbackService.SendContextualWarningAsync
                    ("Could not find any course matching the criteria.", ct: CancellationToken);
            }

            var message = "Found the following courses:\n" +
                          string.Join
                          (
                              '\n',
                              courses.Select
                              (
                                  c => $"- {c.Name} ({c.Code}) {c.Department.GetKey()}\n{c.Homepage ?? string.Empty}"
                                      .Trim()
                              )
                          );
            return await _feedbackService.SendContextualSuccessAsync(message.Trim(), ct: CancellationToken);
        }
    }
}