namespace Application.ApiContracts.User.Requests;

/// <summary>
/// DTO cập nhật thông tin người dùng
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Tên đầy đủ
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Giới tính (Male, Female, Other)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string? PhoneNumber { get; set; }
}
