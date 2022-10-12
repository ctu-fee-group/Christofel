//
//   CoursesInteractivityFormatter.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Courses.Data;
using Christofel.Courses.Extensions;
using Christofel.CoursesLib.Data;
using Christofel.CoursesLib.Database;
using Christofel.Helpers.Localization;
using Christofel.LGPLicensed.Interactivity;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using InflectorExtensions = Humanizer.InflectorExtensions;

namespace Christofel.Courses.Interactivity;

/// <summary>
/// Formats courses interactive messages embeds.
/// </summary>
public class CoursesInteractivityFormatter
{
    private const int MaxItemsPerRow = 5;

    private const int MaxRowsPerMessage = 5;

    private const int MaxMessageLength = 2000;

    private const int MaxButtonLabelLength = 80;

    private readonly LocalizedStringLocalizer<CoursesPlugin> _localizer;
    private readonly ICultureProvider _cultureProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesInteractivityFormatter"/> class.
    /// </summary>
    /// <param name="localizer">The string localizer.</param>
    /// <param name="cultureProvider">The culture provider.</param>
    public CoursesInteractivityFormatter
        (LocalizedStringLocalizer<CoursesPlugin> localizer, ICultureProvider cultureProvider)
    {
        _localizer = localizer;
        _cultureProvider = cultureProvider;
    }

    /// <summary>
    /// Formats a message for joining/leaving given course channels.
    /// </summary>
    /// <remarks>
    /// Makes sure the messages are not more than 2000 characters in length and there are maximum of 25 buttons in one message.
    /// </remarks>
    /// <param name="prepend">The string to prepend at the beginning of the message.</param>
    /// <param name="courses">The courses to put into the message.</param>
    /// <returns>A list of messages representing the messages with course buttons chunked to meet Discord needs.</returns>
    public IReadOnlyList<MessageData> FormatCoursesMessage
    (
        string prepend,
        IReadOnlyList<CourseUserData> courses
    )
    {
        var initialContent = prepend + "\n" + _localizer.Translate($"CHOOSE_COURSE");
        return CreateMessages
        (
            initialContent,
            CoursesFormatter.FormatCourses(courses.Select(x => x.Course))
                .ToDictionary
                (
                    x => courses.First(y => y.Course.ChannelId == x.Key),
                    x => x.Value
                ),
            coursePair =>
            (
                coursePair.Value.Formatted,
                new ButtonComponent
                (
                    coursePair.Key.IsMember ? ButtonComponentStyle.Danger : ButtonComponentStyle.Success,
                    coursePair.Value.Name.Truncate(MaxButtonLabelLength),
                    CustomID: CustomIDHelpers.CreateButtonID
                    (
                        "course",
                        "coursesint",
                        _cultureProvider.CurrentCulture,
                        coursePair.Key.Course.ChannelId.ToString()
                    )
                ),
                coursePair.Key.Course
            )
        );
    }

    /// <summary>
    /// Creates message containing a list of department buttons to select.
    /// </summary>
    /// <param name="prepend">The text to prepend before the message translated.</param>
    /// <param name="departments">The departments to list.</param>
    /// <returns>A list of messages representing the messages with department buttons chunked to meet Discord needs.</returns>
    public IReadOnlyList<MessageData> FormatDepartmentsMessage
    (
        string prepend,
        IReadOnlyList<DepartmentAssignment> departments
    )
    {
        var initialContent = prepend + "\n" + _localizer.Translate($"CHOOSE_DEPARTMENT");
        return CreateMessages
        (
            initialContent,
            departments,
            department =>
                (null, new ButtonComponent
                    (
                        ButtonComponentStyle.Primary,
                        InflectorExtensions
                            .Titleize(department.DepartmentName)
                            .Truncate(MaxButtonLabelLength),
                        CustomID:
                        CustomIDHelpers.CreateButtonID
                        (
                            "department",
                            "coursesint",
                            _cultureProvider.CurrentCulture,
                            department.DepartmentKey
                        )
                    ),
                    null)
        );
    }

    /// <summary>
    /// Format the semesters (previous, current, next) message.
    /// </summary>
    /// <param name="prepend">The text to prepend before the message translated.</param>
    /// <param name="language">The language of the message.</param>
    /// <param name="commandType">The type of the command to execute.</param>
    /// <param name="proceedImmediately">Whether to immediately proceed to the command or to show courses menu.</param>
    /// <returns>A semesters selector message.</returns>
    public MessageData FormatSemesterMessage
    (
        string prepend,
        string language,
        InteractivityCommandType commandType,
        bool proceedImmediately
    )
    {
        return new MessageData
        (
            (prepend + "\n" + _localizer.Translate($"CHOOSE_SEMESTER_{commandType}")).Trim(),
            new[]
            {
                new ActionRowComponent
                (
                    new[]
                    {
                        new ButtonComponent
                        (
                            ButtonComponentStyle.Secondary,
                            _localizer.Translate("SEMESTER_PREVIOUS"),
                            CustomID: CustomIDHelpers.CreateButtonID
                            (
                                "semester",
                                "coursesint",
                                language,
                                commandType.ToString(),
                                proceedImmediately.ToString(),
                                SemesterSelector.Previous.ToString()
                            )
                        ),
                        new ButtonComponent
                        (
                            ButtonComponentStyle.Primary,
                            _localizer.Translate("SEMESTER_CURRENT"),
                            CustomID: CustomIDHelpers.CreateButtonID
                            (
                                "semester",
                                "coursesint",
                                language,
                                commandType.ToString(),
                                proceedImmediately.ToString(),
                                SemesterSelector.Current.ToString()
                            )
                        ),
                        new ButtonComponent
                        (
                            ButtonComponentStyle.Secondary,
                            _localizer.Translate("SEMESTER_NEXT"),
                            CustomID: CustomIDHelpers.CreateButtonID
                            (
                                "semester",
                                "coursesint",
                                language,
                                commandType.ToString(),
                                proceedImmediately.ToString(),
                                SemesterSelector.Next.ToString()
                            )
                        ),
                    }
                )
            }
        );
    }

    /// <summary>
    /// Format the main message of interactivity.
    /// </summary>
    /// <param name="prepend">The text to prepend before the message translated.</param>
    /// <param name="supportedLanguages">The languages that are supported.</param>
    /// <returns>The main message.</returns>
    public MessageData FormatMainMessage(string prepend, params string[] supportedLanguages)
    {
        var languageComponents = new List<IMessageComponent>();
        var languagesRow = new ActionRowComponent(languageComponents);
        foreach (var supportedLanguage in supportedLanguages)
        {
            if (supportedLanguage == _cultureProvider.CurrentCulture)
            {
                continue;
            }

            languageComponents.Add
            (
                new ButtonComponent
                (
                    ButtonComponentStyle.Secondary,
                    _localizer.Delocalized.Translate("SHOW_LANGUAGE", supportedLanguage),
                    CustomID: CustomIDHelpers.CreateButtonID("translate", "coursesint main", supportedLanguage)
                )
            );
        }

        return new MessageData
        (
            (prepend + "\n" + _localizer.Translate("MAIN_MESSAGE_CONTENT")).Trim(),
            new[]
                {
                    new ActionRowComponent
                    (
                        new[]
                        {
                            new ButtonComponent
                            (
                                ButtonComponentStyle.Success,
                                _localizer.Translate("COURSE_BY_KEYS_JOIN_BUTTON"),
                                CustomID: CustomIDHelpers.CreateButtonID
                                    ("keys", "coursesint main", InteractivityCommandType.Join.ToString())
                            ),
                            new ButtonComponent
                            (
                                ButtonComponentStyle.Success,
                                _localizer.Translate("COURSE_BY_SEMESTER_JOIN_BUTTON"),
                                CustomID: CustomIDHelpers.CreateButtonID
                                (
                                    "semesters",
                                    "coursesint main",
                                    _cultureProvider.CurrentCulture,
                                    "true",
                                    InteractivityCommandType.Join.ToString()
                                )
                            ),
                        }
                    ),
                    new ActionRowComponent
                    (
                        new[]
                        {
                            new ButtonComponent
                            (
                                ButtonComponentStyle.Danger,
                                _localizer.Translate("COURSE_BY_KEYS_LEAVE_BUTTON"),
                                CustomID: CustomIDHelpers.CreateButtonID
                                (
                                    "keys",
                                    "coursesint main",
                                    _cultureProvider.CurrentCulture,
                                    InteractivityCommandType.Leave.ToString()
                                )
                            ),
                            new ButtonComponent
                            (
                                ButtonComponentStyle.Danger,
                                _localizer.Translate("COURSE_BY_SEMESTER_LEAVE_BUTTON"),
                                CustomID: CustomIDHelpers.CreateButtonID
                                (
                                    "semesters",
                                    "coursesint main",
                                    _cultureProvider.CurrentCulture,
                                    "true",
                                    InteractivityCommandType.Leave.ToString()
                                )
                            ),
                        }
                    ),
                    new ActionRowComponent
                    (
                        new[]
                        {
                            new ButtonComponent
                            (
                                ButtonComponentStyle.Secondary,
                                _localizer.Translate("SEARCH_COURSES_BUTTON"),
                                CustomID: CustomIDHelpers.CreateButtonID
                                (
                                    "search",
                                    "coursesint main",
                                    _cultureProvider.CurrentCulture
                                )
                            ),
                            new ButtonComponent
                            (
                                ButtonComponentStyle.Secondary,
                                _localizer.Translate("SEARCH_COURSES_BY_DEPARTMENT_BUTTON"),
                                CustomID: CustomIDHelpers.CreateButtonID
                                (
                                    "departments",
                                    "coursesint main",
                                    _cultureProvider.CurrentCulture
                                )
                            ),
                            new ButtonComponent
                            (
                                ButtonComponentStyle.Secondary,
                                _localizer.Translate("SEARCH_COURSES_BY_SEMESTER_BUTTON"),
                                CustomID: CustomIDHelpers.CreateButtonID
                                (
                                    "semesters",
                                    "coursesint main",
                                    _cultureProvider.CurrentCulture,
                                    "false",
                                    InteractivityCommandType.Toggle.ToString()
                                )
                            ),
                        }
                    ),
                    languagesRow
                }
                .Where(x => x.Components.Count > 0)
                .ToArray()
        );
    }

    private IReadOnlyList<MessageData> CreateMessages<T>
    (
        string initialContent,
        IEnumerable<T> data,
        Func<T, (string? AppendContent, IMessageComponent? AppendComponent, CourseAssignment? CourseAssignment)>
            generateAppend
    )
    {
        var messages = new List<MessageData>();

        var currentContent = initialContent;
        var currentCourseAssignments = new List<CourseAssignment>();
        var currentComponents = new List<IMessageComponent>();
        var currentRowComponents = new List<IMessageComponent>();

        foreach (var currentData in data)
        {
            if (ShouldChunk(currentContent, string.Empty, currentComponents))
            {
                if (currentComponents.Count != 0 && ((ActionRowComponent)currentComponents[^1]).Components.Count == 0)
                {
                    currentComponents.Remove(currentComponents[^1]);
                }

                messages.Add(new MessageData(currentContent.Trim(), currentComponents));
                currentContent = initialContent;
                currentComponents = new List<IMessageComponent>();
                currentCourseAssignments = new List<CourseAssignment>();
            }

            if (ShouldCreateNewRow(currentComponents))
            {
                currentRowComponents = new List<IMessageComponent>();
                currentComponents.Add(new ActionRowComponent(currentRowComponents));
            }

            var generated = generateAppend(currentData);

            if (generated.AppendComponent is not null)
            {
                currentRowComponents.Add(generated.AppendComponent);
            }

            if (generated.AppendContent is not null)
            {
                currentContent += "\n" + generated.AppendContent;
            }

            if (generated.CourseAssignment is not null)
            {
                currentCourseAssignments.Add(generated.CourseAssignment);
            }
        }

        if (currentComponents.Count != 0)
        {
            messages.Add
            (
                new MessageData
                (
                    currentContent,
                    currentComponents,
                    currentCourseAssignments.Count > 0 ? currentCourseAssignments : null
                )
            );
        }

        if (currentComponents.Count != 0 && ((ActionRowComponent)currentComponents[^1]).Components.Count == 0)
        {
            currentComponents.Remove(currentComponents[^1]);
        }

        return messages;
    }

    private bool ShouldChunk(string content, string appendContent, IReadOnlyList<IMessageComponent> messageComponents)
    {
        if (content.Length + appendContent.Length + 1 > MaxMessageLength)
        {
            return true;
        }

        return messageComponents.Count == MaxRowsPerMessage
            && ((ActionRowComponent)messageComponents[^1]).Components.Count >= MaxItemsPerRow;
    }

    private bool ShouldCreateNewRow(IReadOnlyList<IMessageComponent> messageComponents)
        => messageComponents.Count == 0
            || ((ActionRowComponent)messageComponents[^1]).Components.Count >= MaxItemsPerRow;
}