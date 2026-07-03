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

        public int? ProductVariantId { get; set; }
        
        public string? VariantName { get; set; }

        public int? ProductVariantColorId { get; set; }

        public string? ColorName { get; set; }

        public string? BrandName { get; set; }

        public string? WarrantyPeriod { get; set; }

        public bool IsActive { get; set; }

        public List<VehicleDocumentResponse> Documents { get; set; } = [];
    }
}
