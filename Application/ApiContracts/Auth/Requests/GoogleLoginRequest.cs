
namespace Application.ApiContracts.Auth.Requests;

/// <summary>
/// DTO cho đăng nhập bằng Google
/// </summary>
public class GoogleLoginRequest
{
    /// <summary>
    /// ID Token từ Google
    /// </summary>
    public string IdToken { get; set; } = string.Empty;
}
