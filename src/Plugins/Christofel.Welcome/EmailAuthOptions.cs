//
//  EmailAuthOptions.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Welcome;

/// <summary>
/// Options for sending an email.
/// </summary>
public class EmailAuthOptions
{
    /// <summary>
    /// Gets or sets the smtp server.
    /// </summary>
    public string Smtp { get; set; } = null!;

    /// <summary>
    /// Gets or sets the port of the smtp server.
    /// </summary>
    public int SmtpPort { get; set; } = 597;

    /// <summary>
    /// Gets or sets the sender of the email.
    /// </summary>
    public string Sender { get; set; } = null!;

    /// <summary>
    /// Gets or sets the username to log in with.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Gets or sets the password to log in with.
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Gets or sets the subject to send.
    /// </summary>
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Gets or sets the content of the email.
    /// </summary>
    public string Content { get; set; } = null!;

    public string AuthMessageContent { get; set; } = null!;

    public string AuthButtonContent { get; set; } = null!;
}