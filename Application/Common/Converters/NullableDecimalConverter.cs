using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Common.Converters;

/// <summary>
/// Provides a custom JSON converter for nullable decimals that handles empty strings by returning null.
/// </summary>
public class NullableDecimalConverter : JsonConverter<decimal?>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if(reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if(string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            if(decimal.TryParse(stringValue, out var result))
            {
                return result;
            }

            throw new JsonException($"Unable to parse '{stringValue}' as a decimal.");
        }

        if(reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetDecimal();
        }

        if(reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        throw new JsonException($"Unexpected token parsing decimal. Expected String or Number, got {reader.TokenType}.");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if(value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
        } else
        {
            writer.WriteNullValue();
        }
    }
}
