
namespace Application.ApiContracts.Auth.Responses;

/// <summary>
/// Response trả về khi đăng nhập thành công
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Access Token (JWT)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian hết hạn của Access Token
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Refresh Token (Only for internal use to set cookie)
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string? RefreshToken { get; set; }
}
