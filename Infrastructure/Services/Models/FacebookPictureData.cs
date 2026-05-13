using System.Text.Json.Serialization;

namespace Infrastructure.Services.Models;

public class FacebookPictureData
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
