namespace Application.ApiContracts.Auth;

/// <summary>
/// DTO cho đăng ký tài khoản mới
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Tên đăng nhập
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Tên đầy đủ
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Giới tính (Male, Female, Other)
    /// </summary>
    public string Gender { get; set; } = "Male";
}
