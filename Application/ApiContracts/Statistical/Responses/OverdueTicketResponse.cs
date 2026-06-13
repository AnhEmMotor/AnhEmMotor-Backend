using System;

namespace Application.ApiContracts.Statistical.Responses;

public class OverdueTicketResponse
{
    public int TicketId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateTimeOffset ExpectedCompletionTime { get; set; }

    public string Status { get; set; } = string.Empty;
}
