using System.Collections.Generic;
using System;

namespace Application.ApiContracts.Sales.Returns.Responses;

public class ReturnRequestResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string OriginalTrackingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? CancelReason { get; set; }
    public string? Note { get; set; }
    public string? ReturnAction { get; set; }
    public string? RejectionReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? InspectedAt { get; set; }
    public List<ReturnRequestItemResponse> Items { get; set; } = new();
}
