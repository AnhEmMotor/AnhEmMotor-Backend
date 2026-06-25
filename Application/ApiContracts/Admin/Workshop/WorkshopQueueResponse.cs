namespace Application.ApiContracts.Admin.Workshop
{
    public record WorkshopQueueResponse(
        int VehicleId,
        string LicensePlate,
        string Model,
        string Status,
        string PendingPart,
        DateTime ExpectedPartArrival,
        string LastLog);

    public record UpdateWorkshopStatusRequest(string NewStatus, string Note);
}
