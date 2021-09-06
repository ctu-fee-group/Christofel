using Remora.Discord.API.Abstractions.Objects;

namespace Christofel.ReactHandler.Formatters
{
    public static class EmojiFormatter
    {
        public static string GetEmojiString(IPartialEmoji emoji)
        {
            string value = "";

            if (emoji.Name.IsDefined(out var name))
            {
                value = name;
            }

            if (emoji.ID.IsDefined(out var id))
            {
                return $"<:{id}:{value}>";
            }

            return value;
        }
    }
}