namespace Application.Interfaces.Authentication;

/// <summary>
/// Interface đánh dấu request có chứa UserId.
/// PipelineBehavior sẽ tự động gán UserId từ HttpContext vào property này.
/// </summary>
public interface IHaveUserId
{
    /// <summary>
    /// ID của người dùng thực hiện request
    /// </summary>
    Guid UserId { get; set; }
}
