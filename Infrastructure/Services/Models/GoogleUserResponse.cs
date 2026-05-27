using System.Text.Json.Serialization;

namespace Infrastructure.Services.Models;

public class GoogleUserResponse
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("picture")]
    public string? Picture { get; set; }

    [JsonPropertyName("sub")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("aud")]
    public string Audience { get; set; } = string.Empty;
}
