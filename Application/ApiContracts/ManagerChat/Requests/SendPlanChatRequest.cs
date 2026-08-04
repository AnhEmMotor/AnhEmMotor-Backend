namespace Application.ApiContracts.ManagerChat.Requests;

public class SendPlanChatRequest
{
    public string Content { get; set; } = string.Empty;

    /// <summary>Có khi user gõ vào ô bình luận riêng của 1 bước — không cần LLM diễn giải,
    /// coi thẳng là 1 bình luận cho đúng bước đó.</summary>
    public string? TargetStepId { get; set; }
}
