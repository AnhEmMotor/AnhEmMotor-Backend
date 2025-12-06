namespace Domain.Constants;

/// <summary>
/// Các giá trị trạng thái người dùng
/// </summary>
public static class UserStatus
{
    /// <summary>
    /// Không kích hoạt
    /// </summary>
    public const string Inactive = "Inactive";

    /// <summary>
    /// Đang hoạt động
    /// </summary>
    public const string Active = "Active";

    /// <summary>
    /// Bị cấm
    /// </summary>
    public const string Banned = "Banned";

    /// <summary>
    /// Bị tạm dừng
    /// </summary>
    public const string Suspended = "Suspended";

    /// <summary>
    /// Danh sách tất cả trạng thái hợp lệ
    /// </summary>
    public static readonly List<string> All = [ Inactive, Active, Banned, Suspended ];

    /// <summary>
    /// Kiểm tra xem trạng thái có hợp lệ không
    /// </summary>
    public static bool IsValid(string? value) => !string.IsNullOrWhiteSpace(value) && All.Contains(value);
}
