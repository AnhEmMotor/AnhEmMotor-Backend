namespace Application.ApiContracts.Evaluation.Requests;

public class CreateServiceEvaluationReplyRequest
{
    public int EvaluationId { get; set; }

    public string Message { get; set; } = string.Empty;

    // optional - để UI có thể map giống Backup.md
    public bool MarkAsProcessed { get; set; } = true;
}

