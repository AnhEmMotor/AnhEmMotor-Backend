namespace Application.ApiContracts.Auth;

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
}
