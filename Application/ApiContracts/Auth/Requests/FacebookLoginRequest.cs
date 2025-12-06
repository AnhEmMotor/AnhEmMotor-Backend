namespace Application.ApiContracts.Auth.Requests;

/// <summary>
/// DTO cho đăng nhập bằng Facebook
/// </summary>
public class FacebookLoginRequest
{
    /// <summary>
    /// Access Token từ Facebook
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
}
