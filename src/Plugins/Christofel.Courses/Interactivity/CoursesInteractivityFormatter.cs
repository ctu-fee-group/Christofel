//
//   CoursesInteractivityFormatter.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.Courses.Data;
using Christofel.Courses.Extensions;
using Christofel.CoursesLib.Database;
using Christofel.LGPLicensed.Interactivity;
using Kos.Data;
using Microsoft.Extensions.Localization;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Feedback.Messages;

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

    private readonly IStringLocalizer<CoursesPlugin> _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoursesInteractivityFormatter"/> class.
    /// </summary>
    /// <param name="localizer">The string localizer.</param>
    public CoursesInteractivityFormatter(IStringLocalizer<CoursesPlugin> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Formats a message for joining/leaving given course channels.
    /// </summary>
    /// <remarks>
    /// Makes sure the messages are not more than 2000 characters in length and there are maximum of 25 buttons in one message.
    /// </remarks>
    /// <param name="language">The language.</param>
    /// <param name="prepend">The string to prepend at the beginning of the message.</param>
    /// <param name="courses">The courses to put into the message.</param>
    /// <param name="commandType">The type of the command (join/leave/toggle) to execute.</param>
    /// <returns>A list of messages representing the messages with course buttons chunked to meet Discord needs.</returns>
    public IReadOnlyList<MessageData> FormatCoursesMessage
    (
        string language,
        string prepend,
        IReadOnlyList<CourseAssignment> courses,
        InteractivityCommandType commandType
    )
    {
        var initialContent = prepend + "\n" + _localizer.Translate($"CHOOSE_COURSE_{commandType}", language);
        return CreateMessages
        (
            initialContent,
            CoursesFormatter.FormatCourses(courses),
            coursePair =>
            (
                coursePair.Value.Formatted,
                new ButtonComponent
                (
                    ButtonComponentStyle.Primary,
                    coursePair.Value.Name.Truncate(MaxButtonLabelLength),
                    CustomID: CustomIDHelpers.CreateButtonID
                    (
                        "course",
                        "coursesint",
                        language,
                        commandType.ToString(),
                        coursePair.Key.Value.ToString()
                    )
                )
            )
        );
    }

    /// <summary>
    /// Creates message containing a list of department buttons to select.
    /// </summary>
    /// <param name="prepend">The text to prepend before the message translated.</param>
    /// <param name="departments">The departments to list.</param>
    /// <param name="language">The language of the message.</param>
    /// <param name="commandType">The type of the command to execute.</param>
    /// <returns>A list of messages representing the messages with department buttons chunked to meet Discord needs.</returns>
    public IReadOnlyList<MessageData> FormatDepartmentsMessage
    (
        string prepend,
        IReadOnlyList<DepartmentAssignment> departments,
        string language,
        InteractivityCommandType commandType
    )
    {
        var initialContent = prepend + "\n" + _localizer.Translate($"CHOOSE_DEPARTMENT_{commandType}", language);
        return CreateMessages
        (
            initialContent,
            departments,
            department =>
                (null, new ButtonComponent
                (
                    ButtonComponentStyle.Primary,
                    department.DepartmentName.Truncate(MaxButtonLabelLength),
                    CustomID:
                    CustomIDHelpers.CreateButtonID
                    (
                        "department",
                        "coursesint",
                        language,
                        commandType.ToString(),
                        department.DepartmentKey
                    )
                ))
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
            (prepend + "\n" + _localizer.Translate($"CHOOSE_SEMESTER_{commandType}", language)).Trim(),
            new[]
            {
                new ActionRowComponent
                (
                    new[]
                    {
                        new ButtonComponent
                        (
                            ButtonComponentStyle.Secondary,
                            _localizer.Translate("SEMESTER_PREVIOUS", language),
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
                            _localizer.Translate("SEMESTER_CURRENT", language),
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
                            _localizer.Translate("SEMESTER_NEXT", language),
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
    /// <param name="language">The language of the message.</param>
    /// <param name="supportedLanguages">The languages that are supported.</param>
    /// <returns>The main message.</returns>
    public MessageData FormatMainMessage(string prepend, string language, params string[] supportedLanguages)
    {
        var languageComponents = new List<IMessageComponent>();
        var languagesRow = new ActionRowComponent(languageComponents);
        foreach (var supportedLanguage in supportedLanguages)
        {
            if (supportedLanguage == language)
            {
                continue;
            }

            languageComponents.Add
            (
                new ButtonComponent
                (
                    ButtonComponentStyle.Secondary,
                    _localizer.Translate("SHOW_LANGUAGE", supportedLanguage),
                    CustomID: CustomIDHelpers.CreateButtonID("translate", "coursesint main", supportedLanguage)
                )
            );
        }

        return new MessageData
        (
            (prepend + "\n" + _localizer.Translate("MAIN_MESSAGE_CONTENT", language)).Trim(),
            new[]
            {
                new ActionRowComponent
                (
                    new[]
                    {
                        new ButtonComponent
                        (
                            ButtonComponentStyle.Success,
                            _localizer.Translate("COURSE_BY_KEYS_JOIN_BUTTON", language),
                            CustomID: CustomIDHelpers.CreateButtonID
                                ("keys", "coursesint main", language, InteractivityCommandType.Join.ToString())
                        ),
                        new ButtonComponent
                        (
                            ButtonComponentStyle.Success,
                            _localizer.Translate("COURSE_BY_DEPARTMENT_JOIN_BUTTON", language),
                            CustomID: CustomIDHelpers.CreateButtonID
                                ("departments", "coursesint main", language, InteractivityCommandType.Join.ToString())
                        ),
                        new ButtonComponent
                        (
                            ButtonComponentStyle.Success,
                            _localizer.Translate("COURSE_BY_SEMESTER_JOIN_BUTTON", language),
                            CustomID: CustomIDHelpers.CreateButtonID
                            (
                                "semesters",
                                "coursesint main",
                                language,
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
                            _localizer.Translate("COURSE_BY_KEYS_LEAVE_BUTTON", language),
                            CustomID: CustomIDHelpers.CreateButtonID
                            (
                                "keys",
                                "coursesint main",
                                language,
                                InteractivityCommandType.Leave.ToString()
                            )
                        ),
                        new ButtonComponent
                        (
                            ButtonComponentStyle.Danger,
                            _localizer.Translate("COURSE_BY_DEPARTMENT_LEAVE_BUTTON", language),
                            CustomID: CustomIDHelpers.CreateButtonID
                            (
                                "departments",
                                "coursesint main",
                                language,
                                InteractivityCommandType.Leave.ToString()
                            )
                        ),
                        new ButtonComponent
                        (
                            ButtonComponentStyle.Danger,
                            _localizer.Translate("COURSE_BY_SEMESTER_LEAVE_BUTTON", language),
                            CustomID: CustomIDHelpers.CreateButtonID
                            (
                                "semesters",
                                "coursesint main",
                                language,
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
                            _localizer.Translate("SEARCH_COURSES_BUTTON", language),
                            CustomID: CustomIDHelpers.CreateButtonID
                            (
                                "search",
                                "coursesint main",
                                language
                            )
                        ),
                        new ButtonComponent
                        (
                            ButtonComponentStyle.Danger,
                            _localizer.Translate("SEARCH_COURSES_BY_SEMESTER_BUTTON", language),
                            CustomID: CustomIDHelpers.CreateButtonID
                            (
                                "semesters",
                                "coursesint main",
                                language,
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
        Func<T, (string? AppendContent, IMessageComponent? AppendComponent)> generateAppend
    )
    {
        var messages = new List<MessageData>();

        var currentContent = initialContent;
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
                currentContent = string.Empty;
                currentComponents = new List<IMessageComponent>();
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
        }

        if (currentComponents.Count != 0)
        {
            messages.Add(new MessageData(currentContent, currentComponents));
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