namespace Application.ApiContracts.Statistical.Responses;

public class PartShortageResponse
{
    public int TicketId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int RequiredQuantity { get; set; }
    public int AvailableQuantity { get; set; }
}
