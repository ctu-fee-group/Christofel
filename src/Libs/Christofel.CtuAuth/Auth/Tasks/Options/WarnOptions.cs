//
//   WarnOptions.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Christofel.CtuAuth.Auth.Tasks.Options
{
    /// <summary>
    /// The options for <see cref="SendNoRolesMessageAuthTask"/>.
    /// </summary>
    public class WarnOptions
    {
        /// <summary>
        /// Gets or sets message to be sent.
        /// </summary>
        public string NoRolesMessage { get; set; } = null!;
    }
}