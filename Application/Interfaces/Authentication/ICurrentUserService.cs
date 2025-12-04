namespace Application.Interfaces.Authentication;

/// <summary>
/// Service lấy thông tin người dùng hiện tại
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID của người dùng hiện tại (nếu đã đăng nhập)
    /// </summary>
    Guid? UserId { get; }
}
