//
//   AllowedMentionsHelper.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Christofel.BaseLib.Implementations.Helpers
{
    public static class AllowedMentionsHelper
    {
        public static AllowedMentions None => new AllowedMentions
            (Roles: new List<Snowflake>(), Users: new List<Snowflake>());

        public static AllowedMentions All => new AllowedMentions();
    }
}