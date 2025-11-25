using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAPI.Converters;

/// <summary>
/// Provides a custom JSON converter for strings that substitutes empty strings for null values during deserialization
/// and omits empty strings during serialization.
/// </summary>
/// <remarks>
/// Use this converter to ensure that deserialized string properties are never null, but instead default to an empty
/// string. When serializing, properties with null or empty string values are not written to the JSON output. This
/// behavior can help avoid null reference issues and reduce JSON payload size for optional string properties.
/// </remarks>
public class EmptyStringConverter : JsonConverter<string>
{
    /// <summary>
    /// Reads a JSON string value from the specified reader and returns it as a .NET string.
    /// </summary>
    /// <param name="reader">
    /// A reference to the <see cref="Utf8JsonReader"/> positioned at the JSON value to read. The reader must be at a
    /// string token.
    /// </param>
    /// <param name="typeToConvert">The type of the value to convert. This parameter is ignored for string values.</param>
    /// <param name="options">Options to control the behavior of the JSON serializer. This parameter is not used for string values.</param>
    /// <returns>The string value read from the JSON input. Returns <see langword="string.Empty"/> if the JSON value is null.</returns>
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    { return reader.GetString() ?? string.Empty; }

    /// <summary>
    /// Writes the specified string value to the JSON output using the provided writer, if the value is not null or
    /// empty.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> instance used to write the JSON value.</param>
    /// <param name="value">The string value to write. If null or empty, no value is written.</param>
    /// <param name="options">Options to control JSON serialization behavior. This parameter can be used to customize serialization features.</param>
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        if(string.IsNullOrEmpty(value))
        {
            return;
        }

        writer.WriteStringValue(value);
    }
}