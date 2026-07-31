namespace Domain.Constants;

public static class PlanStepStatus
{
    public const string Pending = "pending";
    public const string Running = "running";
    public const string Done = "done";
    public const string Failed = "failed";
    public const string Skipped = "skipped";

    /// <summary>Tool cần cho bước này đã bị gỡ khỏi registry giữa lúc chờ duyệt (Stage 17.8).</summary>
    public const string Invalid = "invalid";
}
