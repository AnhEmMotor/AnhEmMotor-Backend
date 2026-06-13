namespace Application.ApiContracts.Evaluation.Responses;

public class ServiceEvaluationListResponse
{
    public List<ServiceEvaluationListRowResponse> Items { get; set; } = new();

    public int TotalCount { get; set; }
}

