using System.Collections.Generic;

namespace Application.ApiContracts.WarrantyClaim.Responses
{
    public class WarrantyClaimDetailResponse
    {
        public int Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string IssueDescription { get; set; } = string.Empty;
        public List<string> MediaUrls { get; set; } = new();
        public string? ServiceCenterName { get; set; }
        public string? ManufacturerClaimNumber { get; set; }
        public string? ManufacturerDecision { get; set; }
        public bool IsRecall { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? VehicleVin { get; set; }
        public string? VehiclePlate { get; set; }
        public string? VehicleColor { get; set; }
        public string? VehicleYear { get; set; }
        public string? WarrantyRemaining { get; set; }
        public List<WarrantyClaimPartResponse> Parts { get; set; } = new();
    }
}
