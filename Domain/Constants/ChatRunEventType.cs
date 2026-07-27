namespace Domain.Constants;

public static class ChatRunEventType
{
    public const string RunStarted = "run_started";
    public const string TextDelta = "text_delta";
    public const string Error = "error";
    public const string RunCompleted = "run_completed";
    public const string RunCancelled = "run_cancelled";
    public const string RunHeartbeat = "run_heartbeat";
}
