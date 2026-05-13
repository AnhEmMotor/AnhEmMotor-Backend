using System.Text.Json.Serialization;

namespace Infrastructure.Services.Models;

public class FacebookUserResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("picture")]
    public FacebookPicture? Picture { get; set; }
}
