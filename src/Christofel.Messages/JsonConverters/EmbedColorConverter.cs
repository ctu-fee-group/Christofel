using System;
using System.Drawing;
using Newtonsoft.Json;

namespace Christofel.Messages.JsonConverters
{
    public class EmbedColorConverter : JsonConverter<Color?>
    {
        public override void WriteJson(JsonWriter writer, Color? value, JsonSerializer serializer)
        {
            writer.WriteValueAsync(value?.ToArgb() ?? 0);
        }

        public override Color? ReadJson(JsonReader reader, Type objectType, Color? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            return Color.FromArgb(((int?)reader.Value) ?? 0);
        }
    }
}