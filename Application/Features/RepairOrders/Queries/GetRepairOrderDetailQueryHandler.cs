using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.RepairOrders.Queries;

public class GetRepairOrderDetailQueryHandler(
    IMaintenanceHistoryReadRepository repo) : IRequestHandler<GetRepairOrderDetailQuery, Result<RepairOrderResponse>>
{
    public async Task<Result<RepairOrderResponse>> Handle(GetRepairOrderDetailQuery req, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(req.Id, ct);
        if (entity is null)
            return Result<RepairOrderResponse>.Failure([Error.NotFound($"Không tìm thấy lệnh sửa chữa id={req.Id}", "Id")]);

        return Result<RepairOrderResponse>.Success(new RepairOrderResponse
        {
            Id = entity.Id,
            MaintenanceNumber = entity.MaintenanceNumber,
            VehicleId = entity.VehicleId,
            MaintenanceDate = entity.MaintenanceDate,
            Description = entity.Description,
            Mileage = entity.Mileage,
            TechnicianId = entity.TechnicianId,
            PartsCost = entity.PartsCost,
            LaborCost = entity.LaborCost,
            TotalCost = entity.TotalCost,
            PartsJson = entity.PartsJson,
            NextMaintenanceDate = entity.NextMaintenanceDate,
            NextMaintenanceOdo = entity.NextMaintenanceOdo,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        });
    }
}
