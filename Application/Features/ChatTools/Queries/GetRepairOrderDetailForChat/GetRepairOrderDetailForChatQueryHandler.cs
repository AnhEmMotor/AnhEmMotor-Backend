using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetRepairOrderDetailForChat;

public class GetRepairOrderDetailForChatQueryHandler(
    IMaintenanceHistoryReadRepository repo,
    IVehicleReadRepository vehicleRepo,
    IEmployeeReadRepository employeeRepo,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetRepairOrderDetailForChatQuery, Result<ChatToolEnvelope<ChatRepairOrderDetailDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatRepairOrderDetailDto>>> Handle(
        GetRepairOrderDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await repo.GetByIdAsync(request.RepairOrderId, cancellationToken).ConfigureAwait(false);
        if (entity == null)
        {
            return Result<ChatToolEnvelope<ChatRepairOrderDetailDto>>.Failure(
                Error.NotFound("Không tìm thấy phiếu sửa chữa"));
        }

        var vehicle = await vehicleRepo.GetByIdAsync(entity.VehicleId, cancellationToken).ConfigureAwait(false);
        string? vehicleInfo = vehicle != null
            ? (!string.IsNullOrEmpty(vehicle.LicensePlate) ? vehicle.LicensePlate : vehicle.VinNumber)
            : null;
        string? customerName = vehicle?.Lead?.FullName;

        string? technicianName = null;
        if (entity.TechnicianId.HasValue)
        {
            var emp = await employeeRepo.GetByIdAsync(entity.TechnicianId.Value, cancellationToken).ConfigureAwait(false);
            technicianName = emp?.User?.FullName;
        }

        var dto = new ChatRepairOrderDetailDto
        {
            RepairOrderId = entity.Id,
            MaintenanceNumber = entity.MaintenanceNumber,
            VehicleInfo = vehicleInfo,
            CustomerName = customerName,
            TechnicianName = technicianName,
            Description = entity.Description,
            PartsJson = entity.PartsJson,
            PartsCost = entity.PartsCost,
            LaborCost = entity.LaborCost,
            TotalCost = entity.TotalCost,
            MaintenanceDate = entity.MaintenanceDate
        };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IMaintenanceHistoryReadRepository.GetByIdAsync",
            new Dictionary<string, string>(),
            "phieu-sua-chua",
            null);
        return ChatToolEnvelope<ChatRepairOrderDetailDto>.WrapSingle(dto, meta);
    }
}
