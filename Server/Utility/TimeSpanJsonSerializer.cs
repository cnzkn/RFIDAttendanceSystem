using System.Globalization;

namespace CloudAPI.Utility;

public class TimeSpanJsonSerializer : JsonConverter<TimeSpan?>
{
    public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert != typeof(TimeSpan)) 
        {
            throw new JsonException($"{nameof(TimeSpanJsonSerializer)} cannot serialize type {typeToConvert.FullName}");
        }
        
        if (TimeSpan.TryParseExact(reader.GetString(), "c", CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        else
        {
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString("c", CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
