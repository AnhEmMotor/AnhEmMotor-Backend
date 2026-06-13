namespace Application.ApiContracts.Statistical.Responses;

public class TechnicianRankingResponse
{
    public string TechnicianName { get; set; } = string.Empty;

    public int CompletedTickets { get; set; }

    public decimal TotalRevenue { get; set; }

    public double ComplaintRate { get; set; }
}
