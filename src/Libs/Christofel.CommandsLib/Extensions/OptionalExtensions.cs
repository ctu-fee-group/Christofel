using Remora.Discord.Core;

namespace Christofel.CommandsLib.Extensions
{
    public static class OptionalExtensions
    {
        public static bool CheckOptionalBoolMatches(this Optional<bool> left, Optional<bool> right, bool @default)
        {
            return (left.HasValue ? left.Value : @default) == (right.HasValue ? right.Value : @default);
        }
    }
}