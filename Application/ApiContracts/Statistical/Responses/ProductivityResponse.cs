namespace Application.ApiContracts.Statistical.Responses;

public class ProductivityResponse
{
    public List<TechnicianStatusResponse> TechnicianStatuses { get; set; } = new();

    public List<TechnicianRankingResponse> TechnicianRankings { get; set; } = new();
}
