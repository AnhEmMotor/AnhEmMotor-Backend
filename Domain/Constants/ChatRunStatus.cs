namespace Domain.Constants;

public static class ChatRunStatus
{
    public const string Pending = "Pending";
    public const string Running = "Running";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Failed = "Failed";
    public const string Orphaned = "Orphaned";

    /// <summary>
    /// Plan đã sinh xong, chờ user duyệt (Stage 10) — không tính vào timeout 5 phút/2 phút của run thường.
    /// </summary>
    public const string AwaitingApproval = "AwaitingApproval";
}
