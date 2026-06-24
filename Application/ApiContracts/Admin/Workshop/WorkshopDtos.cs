using System.Collections.Generic;

namespace Application.ApiContracts.Admin.Workshop
{
    public record WorkshopQueueResponse(
        int VehicleId, 
        string LicensePlate, 
        string Model, 
        string Status, // Tiếp nhận -> Sẵn sàng giao
        string PendingPart, 
        DateTime ExpectedPartArrival, 
        string LastLog);

    public record UpdateWorkshopStatusRequest(string NewStatus, string Note);
}
