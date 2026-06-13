namespace Application.ApiContracts.Statistical.Responses;

public class TechnicianStatusResponse
{
    public string TechnicianName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int? CurrentTicketId { get; set; }
}
