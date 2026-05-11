using System;

namespace Application.ApiContracts.Vehicle.Responses
{
    public class VehicleResponse
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string VinNumber { get; set; } = string.Empty;

        public string EngineNumber { get; set; } = string.Empty;

        public string LicensePlate { get; set; } = string.Empty;

        public DateTimeOffset PurchaseDate { get; set; }

        public int LeadId { get; set; }

        public int? ProductId { get; set; }

        public bool IsActive { get; set; }

        public List<VehicleDocumentResponse> Documents { get; set; } = [];
    }
}
