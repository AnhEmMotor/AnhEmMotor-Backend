using System;

namespace Application.ApiContracts.WarrantyClaim.Responses
{
    public class WarrantyClaimResponse
    {
        public int Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public string? VehiclePlate { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
