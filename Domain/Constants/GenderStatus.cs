namespace Domain.Constants;

/// <summary>
/// Các giá trị giới tính
/// </summary>
public static class GenderStatus
{
    /// <summary>
    /// Nam
    /// </summary>
    public const string Male = "Male";

    /// <summary>
    /// Nữ
    /// </summary>
    public const string Female = "Female";

    /// <summary>
    /// Khác
    /// </summary>
    public const string Other = "Other";

    /// <summary>
    /// Danh sách tất cả giới tính hợp lệ
    /// </summary>
    public static readonly List<string> All = [ Male, Female, Other ];

    /// <summary>
    /// Kiểm tra xem giới tính có hợp lệ không
    /// </summary>
    public static bool IsValid(string? value) => !string.IsNullOrWhiteSpace(value) && All.Contains(value);
}
