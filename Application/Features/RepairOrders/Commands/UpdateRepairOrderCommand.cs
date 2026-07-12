using Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.RepairOrders.Commands;

public record UpdateRepairOrderCommand(
    [Required] int Id,
    int VehicleId,
    DateTimeOffset MaintenanceDate,
    string Description,
    int Mileage,
    int? TechnicianId,
    decimal PartsCost,
    decimal LaborCost,
    string? PartsJson,
    DateTimeOffset? NextMaintenanceDate,
    int? NextMaintenanceOdo
) : IRequest<Result>;
