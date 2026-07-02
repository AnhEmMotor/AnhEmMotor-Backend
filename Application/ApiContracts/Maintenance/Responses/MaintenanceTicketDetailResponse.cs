using System;
using System.Collections.Generic;

namespace Application.ApiContracts.Maintenance.Responses
{
    public class MaintenancePartDto
    {
        public string PartName { get; set; } = string.Empty;
        public string PartCode { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }

    public class MaintenanceTicketDetailResponse
    {
        public int Id { get; set; }
        public string MaintenanceNumber { get; set; } = string.Empty;
        public DateTimeOffset MaintenanceDate { get; set; }
        public int Mileage { get; set; }
        public string Description { get; set; } = string.Empty;
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal TotalCost { get; set; }
        public DateTimeOffset? NextMaintenanceDate { get; set; }
        public int? NextMaintenanceOdo { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? VehicleVin { get; set; }
        public string? VehiclePlate { get; set; }
        public string? VehicleColor { get; set; }
        public string? VehicleYear { get; set; }
        public List<MaintenancePartDto> Parts { get; set; } = new();
    }
}
