//
//   AllowedMentionsHelper.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Christofel.Helpers.Helpers
{
    /// <summary>
    /// Helper for <see cref="AllowedMentions"/>.
    /// </summary>
    public static class AllowedMentionsHelper
    {
        /// <summary>
        /// Gets <see cref="AllowedMentions"/> that represents no one will be mentioned.
        /// </summary>
        public static AllowedMentions None => new AllowedMentions
            (Roles: new List<Snowflake>(), Users: new List<Snowflake>());

        /// <summary>
        /// Gets <see cref="AllowedMentions"/> that represents everyone will be mentioned.
        /// </summary>
        public static AllowedMentions All => new AllowedMentions();
    }
}