using Application.Common.Models;
using Domain.Constants.Permission;
using Domain.Entities;
using MediatR;

namespace Application.Features.RepairOrders.Commands;

public record CreateRepairOrderCommand(
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
) : IRequest<Result<int>>;
