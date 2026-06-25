using System;

namespace Application.ApiContracts.Admin.Logistics
{
    public record OrderPipelineResponse(int OrderId, string CustomerName, string CurrentStage, DateTime LastUpdate);

    public record BottleneckResponse(
        int OrderId,
        string CustomerName,
        string BottleneckReason,
        int DelayHours,
        string Severity);

    public record ShippingMapResponse(
        double Latitude,
        double Longitude,
        string DriverName,
        string DriverPhone,
        DateTime EstimatedArrivalTime);
}
