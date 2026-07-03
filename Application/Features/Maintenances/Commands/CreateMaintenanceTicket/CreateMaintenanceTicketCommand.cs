using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Maintenances.Commands.CreateMaintenanceTicket
{
    public class CreateMaintenancePartInput
    {
        public string PartName { get; set; } = string.Empty;
        public string PartCode { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }

    public class CreateMaintenanceTicketCommand : IRequest<Result<int>>
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public int? TechnicianId { get; set; }
        public decimal LaborCost { get; set; }
        public int CycleKm { get; set; } = 5000;
        public int CycleMonths { get; set; } = 6;
        public List<CreateMaintenancePartInput> Parts { get; set; } = new();
    }
}
