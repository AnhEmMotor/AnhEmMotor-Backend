namespace Application.ApiContracts.UserManager.Requests;

/// <summary>
/// DTO thay đổi trạng thái của nhiều người dùng
/// </summary>
public class ChangeMultipleUsersStatusRequest
{
    /// <summary>
    /// Danh sách ID người dùng
    /// </summary>
    public List<Guid> UserIds { get; set; } = [];

    /// <summary>
    /// Trạng thái: Active, Inactive, Banned, Suspended
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
