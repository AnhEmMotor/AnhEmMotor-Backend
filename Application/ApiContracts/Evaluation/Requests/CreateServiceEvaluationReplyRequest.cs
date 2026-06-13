namespace Application.ApiContracts.Evaluation.Requests;

public class CreateServiceEvaluationReplyRequest
{
    public int EvaluationId { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool MarkAsProcessed { get; set; } = true;
}

