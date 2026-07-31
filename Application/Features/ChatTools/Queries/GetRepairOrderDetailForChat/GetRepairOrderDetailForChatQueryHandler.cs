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
    private const int MaxResults = 5;

    public async Task<Result<ChatToolEnvelope<ChatRepairOrderDetailDto>>> Handle(
        GetRepairOrderDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        // Tìm xe khớp keyword theo biển số/VIN/tên khách hàng (đã có sẵn trong IVehicleReadRepository).
        var vehicles = await vehicleRepo.GetVehiclesAsync(request.Keyword, cancellationToken).ConfigureAwait(false);
        var vehicleDict = vehicles.ToDictionary(v => v.Id, v => v);

        var dtos = new List<ChatRepairOrderDetailDto>();
        if (vehicleDict.Count > 0)
        {
            var histories = new List<Domain.Entities.MaintenanceHistory>();
            foreach (var vehicleId in vehicleDict.Keys)
            {
                var vehicleHistories = await repo.GetByVehicleIdAsync(vehicleId, cancellationToken).ConfigureAwait(false);
                histories.AddRange(vehicleHistories);
            }

            var topHistories = histories
                .OrderByDescending(h => h.MaintenanceDate)
                .Take(MaxResults)
                .ToList();

            var technicianIds = topHistories
                .Where(h => h.TechnicianId.HasValue)
                .Select(h => h.TechnicianId!.Value)
                .Distinct()
                .ToList();
            var technicianNames = new Dictionary<int, string?>();
            foreach (var technicianId in technicianIds)
            {
                var emp = await employeeRepo.GetByIdAsync(technicianId, cancellationToken).ConfigureAwait(false);
                technicianNames[technicianId] = emp?.User?.FullName;
            }

            foreach (var entity in topHistories)
            {
                var vehicle = vehicleDict.GetValueOrDefault(entity.VehicleId);
                string? vehicleInfo = vehicle != null
                    ? (!string.IsNullOrEmpty(vehicle.LicensePlate) ? vehicle.LicensePlate : vehicle.VinNumber)
                    : null;
                string? customerName = vehicle?.Lead?.FullName;
                string? technicianName = entity.TechnicianId.HasValue
                    ? technicianNames.GetValueOrDefault(entity.TechnicianId.Value)
                    : null;

                dtos.Add(
                    new ChatRepairOrderDetailDto
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
                    });
            }
        }

        var inner = new ChatToolResult<ChatRepairOrderDetailDto>(dtos, dtos.Count, false);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IVehicleReadRepository.GetVehiclesAsync + IMaintenanceHistoryReadRepository.GetByVehicleIdAsync",
            new Dictionary<string, string>(),
            "phieu-sua-chua",
            null);
        return ChatToolEnvelope<ChatRepairOrderDetailDto>.Wrap(inner, meta);
    }
}
