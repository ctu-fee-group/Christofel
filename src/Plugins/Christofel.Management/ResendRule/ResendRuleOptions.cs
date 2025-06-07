//
//   ResendRuleOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.Management.ResendRule
{
    /// <summary>
    /// Options for resending messages.
    /// </summary>
    public class ResendRuleOptions
    {
        /// <summary>
        /// Gets or sets the header of the resend message.
        /// </summary>
        public string Header { get; set; } = "Resending message from {channel} sent by {mention}";

        /// <summary>
        /// Gets or sets the format of the resend message.
        /// </summary>
        public string Format { get; set; } = "{header}\n\n{message}";

        /// <summary>
        /// Gets or sets the duration to cache for in seconds.
        /// </summary>
        public int CacheDuration { get; set; } = 60 * 10;
    }
}