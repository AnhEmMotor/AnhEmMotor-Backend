using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Common.Converters;

/// <summary>
/// Provides a custom JSON converter for <see cref="DateTime?" /> that treats an empty or whitespace string as
/// <see langword="null" />, and otherwise parses the value using the current culture or ISO-8601 formats.
/// </summary>
/// <remarks>
/// Some clients (e.g. the Management SPA) submit optional date fields such as a customer's birthday as an empty
/// string (<c>""</c>) when the user has not picked a value. The default System.Text.Json behaviour would reject that
/// payload with a 400 validation error (<c>The JSON value could not be converted to System.Nullable`1[System.DateTime]</c>).
/// This converter normalises those empty strings to <see langword="null" /> so the request can bind successfully.
/// </remarks>
public class NullableDateTimeConverter : JsonConverter<DateTime?>
{
    /// <summary>
    /// Reads a JSON value and converts it to a nullable <see cref="DateTime" />.
    /// </summary>
    /// <param name="reader">The JSON reader positioned at the value to convert.</param>
    /// <param name="typeToConvert">The type of the value to convert (ignored; always <see cref="DateTime?" />).</param>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>
    /// A <see cref="DateTime" /> when the JSON value is a parseable string or ISO date, otherwise
    /// <see langword="null" /> for empty strings or JSON null.
    /// </returns>
    /// <exception cref="JsonException">Thrown when the value is a non-empty string that cannot be parsed as a date/time.</exception>
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }
            if (DateTime.TryParse(
                stringValue,
                CultureInfo.CurrentCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                out var result))
            {
                return result;
            }
            if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out result))
            {
                return result;
            }
            throw new JsonException($"Unable to parse '{stringValue}' as a DateTime.");
        }
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        throw new JsonException($"Unexpected token parsing DateTime. Expected String or Null, got {reader.TokenType}.");
    }

    /// <summary>
    /// Writes a nullable <see cref="DateTime" /> value to the JSON output.
    /// </summary>
    /// <param name="writer">The JSON writer used to write the value.</param>
    /// <param name="value">The nullable date/time value to write.</param>
    /// <param name="options">The JSON serializer options to use.</param>
    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
