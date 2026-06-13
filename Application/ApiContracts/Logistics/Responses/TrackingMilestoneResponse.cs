namespace Application.ApiContracts.Logistics.Responses;

public class TrackingMilestoneResponse
{
    public int Id { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? CarrierLogCode { get; set; }

    public bool IsCurrentLocation { get; set; }

    public bool IsStuck { get; set; }
    public string? Location { get; set; }
    public bool IsCurrent { get; set; }
    public string? StatusType { get; set; } // e.g. "Completed", "InTransit", "Pending"
}