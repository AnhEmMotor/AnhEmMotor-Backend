using Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

/// <summary>
/// Người dùng trong hệ thống
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Tên đầy đủ của người dùng
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Giới tính (Male, Female, Other)
    /// </summary>
    public string Gender { get; set; } = GenderStatus.Male;

    /// <summary>
    /// Refresh Token để lấy Access Token mới
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Thời gian hết hạn của Refresh Token
    /// </summary>
    public DateTimeOffset RefreshTokenExpiryTime { get; set; }

    /// <summary>
    /// Trạng thái của người dùng (Active, Inactive, Banned, Suspended)
    /// </summary>
    public string Status { get; set; } = UserStatus.Active;

    /// <summary>
    /// Thời gian người dùng xoá tài khoản (soft delete) Nếu null = tài khoản chưa bị xoá
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Các vai trò của người dùng
    /// </summary>
    public ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = [];
}
