using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Common.Converters;

public class FlexibleStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out long l))
            {
                return l.ToString();
            }
            if (reader.TryGetDouble(out double d))
            {
                return d.ToString();
            }
            return reader.GetDecimal().ToString();
        }
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        if (reader.TokenType == JsonTokenType.True)
            return "true";
        if (reader.TokenType == JsonTokenType.False)
            return "false";
        using var doc = JsonDocument.ParseValue(ref reader);
        return doc.RootElement.GetRawText();
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        } else
        {
            writer.WriteStringValue(value);
        }
    }
}
