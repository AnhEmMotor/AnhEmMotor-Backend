namespace Application.ApiContracts.Evaluation.Requests;

public class UpdateServiceEvaluationInternalNotesRequest
{
    public int EvaluationId { get; set; }

    public string InternalNotes { get; set; } = string.Empty;
}

