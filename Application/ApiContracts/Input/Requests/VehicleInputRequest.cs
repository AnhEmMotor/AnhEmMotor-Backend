using System;

namespace Application.ApiContracts.Input.Requests
{
    public class VehicleInputRequest
    {
        public string VinNumber { get; set; } = string.Empty;

        public string EngineNumber { get; set; } = string.Empty;

        public string LicensePlate { get; set; } = string.Empty;
    }
}
