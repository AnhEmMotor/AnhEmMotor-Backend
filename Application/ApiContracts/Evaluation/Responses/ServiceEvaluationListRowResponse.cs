namespace Application.ApiContracts.Evaluation.Responses;

public class ServiceEvaluationListRowResponse
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string ReviewMessage { get; set; } = string.Empty;

    public string Criteria { get; set; } = string.Empty; // QualityOfCar | AttitudeOfService

    public string ProcessingStatus { get; set; } = "Unprocessed";

    public string? TechnicianName { get; set; }

    public string? RepairOrderCode { get; set; }
}

