//
//   PRScrapingOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Management.Crons;

/// <summary>
/// Options for <see cref="ScrapePRCron"/>.
/// </summary>
public class PRScrapingOptions
{
    /// <summary>
    /// The url to load the html page from.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The query to find the e-mail links with
    /// Ie. ".contact-info a".
    /// </summary>
    public string EmailQuery { get; set; } = string.Empty;

    /// <summary>
    /// The AssignmentId to assign in the database.
    /// </summary>
    /// <remarks>
    /// If changing this, you will have to manually remove the existing entries in the database.
    /// </remarks>
    public int AssignmentRoleId { get; set; }
}
