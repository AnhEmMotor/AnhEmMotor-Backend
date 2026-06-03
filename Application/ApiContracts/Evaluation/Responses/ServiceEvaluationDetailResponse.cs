namespace Application.ApiContracts.Evaluation.Responses;

public class ServiceEvaluationDetailResponse : ServiceEvaluationListRowResponse
{
    public List<ServiceEvaluationChatMessageResponse> ChatHistory { get; set; } = new();

    public string? DirectReplyText { get; set; }

    public string? InternalNotes { get; set; }
}

