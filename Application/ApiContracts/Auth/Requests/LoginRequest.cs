using System.Text.Json.Serialization;

using Application;
using Application.ApiContracts;
using Application.ApiContracts.Auth;

namespace Application.ApiContracts.Auth.Requests;

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
