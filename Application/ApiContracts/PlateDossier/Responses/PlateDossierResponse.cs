using System;

namespace Application.ApiContracts.PlateDossier.Responses
{
    public class PlateDossierResponse
    {
        public int Id { get; set; }
        public int OutputId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? VehicleName { get; set; }
        public string Status { get; set; } = "Prepare";
        public string? LicensePlate { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal ActualCost { get; set; }
        public decimal ServiceFee { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
