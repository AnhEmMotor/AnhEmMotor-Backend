using System.Collections.Generic;

namespace Application.ApiContracts.Statistical.Responses;

public class UrgentAlertsResponse
{
    public List<OverdueTicketResponse> OverdueTickets { get; set; } = [];
    public List<PartShortageResponse> PartShortages { get; set; } = [];
}
