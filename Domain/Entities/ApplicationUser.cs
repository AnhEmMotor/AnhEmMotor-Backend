using Domain.Enums;
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
    /// Giới tính
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Refresh Token để lấy Access Token mới
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Thời gian hết hạn của Refresh Token
    /// </summary>
    public DateTime RefreshTokenExpiryTime { get; set; }

    /// <summary>
    /// Các vai trò của người dùng
    /// </summary>
    public virtual ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = [];
}
