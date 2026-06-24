using System;
using System.Collections.Generic;

namespace Application.ApiContracts.Admin.Resolution
{
    public record TicketSummaryResponse(
        int Id, 
        string CustomerName, 
        string Subject, 
        string Priority, 
        string Status, 
        DateTime SLADeadline);

    public record ReplyTicketRequest(
        string InternalNote, 
        string PublicReply, 
        string ResolutionStatus);
}
