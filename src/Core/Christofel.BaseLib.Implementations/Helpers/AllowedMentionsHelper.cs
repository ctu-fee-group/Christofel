using System.Collections.Generic;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Christofel.BaseLib.Implementations.Helpers
{
    public static class AllowedMentionsHelper
    {
        public static AllowedMentions None => new AllowedMentions(Roles: new List<Snowflake>(), Users: new List<Snowflake>());
        public static AllowedMentions All => new AllowedMentions();
    }
}