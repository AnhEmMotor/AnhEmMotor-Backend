using System;

namespace Application.ApiContracts.PlateDossier.Responses
{
    public class PlateDossierResponse
    {
        public int Id { get; set; }

        public int? OutputId { get; set; }

        public string DossierNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public string VinNumber { get; set; } = string.Empty;

        public string Status { get; set; } = "Prepare";

        public string? LicensePlate { get; set; }

        public decimal RegistrationFee { get; set; }

        public decimal ActualCost { get; set; }

        public decimal ServiceFee { get; set; }

        public string? Notes { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? CompletedDate { get; set; }
        public string? VehicleName { get; set; }
    }
}

