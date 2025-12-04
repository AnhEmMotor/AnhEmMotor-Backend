namespace Application.ApiContracts.Auth;

/// <summary>
/// DTO cho đăng nhập bằng Username/Email và Password
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username hoặc Email
    /// </summary>
    public string UsernameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
