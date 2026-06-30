using System;

namespace Application.ApiContracts.Maintenance.Responses
{
    public class MaintenanceTicketResponse
    {
        public int Id { get; set; }
        public string MaintenanceNumber { get; set; } = string.Empty;
        public string? VehiclePlate { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTimeOffset MaintenanceDate { get; set; }
        public int Mileage { get; set; }
        public decimal TotalCost { get; set; }
    }
}
