namespace Domain.Constants;

public static class ChatRunEventType
{
    public const string RunStarted = "run_started";
    public const string TextDelta = "text_delta";
    public const string Error = "error";
    public const string RunCompleted = "run_completed";
    public const string RunCancelled = "run_cancelled";
    public const string RunHeartbeat = "run_heartbeat";
    public const string SteeringReceived = "steering_received";
    public const string SteeringApplied = "steering_applied";
    public const string RunRedirected = "run_redirected";
    public const string TurnBoundary = "turn_boundary";
    public const string ToolStart = "tool_start";
    public const string ToolEnd = "tool_end";
    public const string MessageCorrection = "message_correction";
    public const string RunMeta = "run_meta";
    public const string Thinking = "thinking";
    public const string SuggestedPrompt = "suggested_prompt";

    public const string PlanStarted = "plan_started";
    public const string PlanStepAdded = "plan_step_added";
    public const string PlanReady = "plan_ready";
    public const string PlanEdited = "plan_edited";
    public const string PlanApproved = "plan_approved";
    public const string PlanRejected = "plan_rejected";
    public const string PlanStepStarted = "plan_step_started";
    public const string PlanStepCompleted = "plan_step_completed";
    public const string PlanInvalidated = "plan_invalidated";
}
