
namespace Application.ApiContracts.Auth.Responses;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string? RefreshToken { get; set; }
}
