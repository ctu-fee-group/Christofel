using System;
using Discord;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Christofel.Messages.JsonConverters
{
    public class EmbedColorConverter : JsonConverter<Color?>
    {
        public override void WriteJson(JsonWriter writer, Color? value, JsonSerializer serializer)
        {
            writer.WriteValueAsync(value?.RawValue ?? 0);
        }

        public override Color? ReadJson(JsonReader reader, Type objectType, Color? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            return new Color((Convert.ToUInt32((long?) reader.Value ?? 0)));
        }
    }
}