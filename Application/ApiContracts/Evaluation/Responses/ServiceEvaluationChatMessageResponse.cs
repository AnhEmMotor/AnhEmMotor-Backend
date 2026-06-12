namespace Application.ApiContracts.Evaluation.Responses;

public class ServiceEvaluationChatMessageResponse
{
    public string Id { get; set; } = string.Empty;

    // Customer | Admin | System
    public string Sender { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}

