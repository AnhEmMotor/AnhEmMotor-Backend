using System;
using System.Collections.Generic;

namespace Application.ApiContracts.RepairOrder.Responses
{
    public class RepairOrderResponse
    {
        public int Id { get; set; }
        public int? VehicleId { get; set; }
        public string? LicensePlate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public string Description { get; set; } = string.Empty;
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "Unpaid";
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset? CompletedDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<RepairOrderDetailResponse> Details { get; set; } = new();
    }
}
