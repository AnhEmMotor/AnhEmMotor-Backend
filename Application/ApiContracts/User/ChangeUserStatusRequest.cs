namespace Application.ApiContracts.User;

/// <summary>
/// DTO thay đổi trạng thái của người dùng
/// </summary>
public class ChangeUserStatusRequest
{
    /// <summary>
    /// Trạng thái: Active, Inactive, Banned, Suspended
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
