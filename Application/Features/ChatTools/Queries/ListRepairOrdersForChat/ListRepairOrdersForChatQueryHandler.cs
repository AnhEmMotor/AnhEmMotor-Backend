using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListRepairOrdersForChat;

public class ListRepairOrdersForChatQueryHandler(
    IMaintenanceHistoryReadRepository repo,
    IVehicleReadRepository vehicleRepo,
    IServerDateProvider dateProvider) : IRequestHandler<ListRepairOrdersForChatQuery, Result<ChatToolEnvelope<ChatRepairOrderListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatRepairOrderListItemDto>>> Handle(
        ListRepairOrdersForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel { Sorts = "-MaintenanceDate", Page = 1, PageSize = limit };
        var paged = await repo.GetPagedAsync<RepairOrderResponse>(sieveModel, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var items = paged.Items ?? [];
        if (items.Count > 0)
        {
            var vehicleIds = items.Select(x => x.VehicleId).Distinct().ToList();
            var vehicles = await vehicleRepo.GetByIdsAsync(vehicleIds, cancellationToken).ConfigureAwait(false);
            var vehicleDict = vehicles.ToDictionary(v => v.Id, v => v);
            foreach (var item in items)
            {
                if (vehicleDict.TryGetValue(item.VehicleId, out var vehicle))
                {
                    item.VehicleInfo = !string.IsNullOrEmpty(vehicle.LicensePlate)
                        ? vehicle.LicensePlate
                        : vehicle.VinNumber;
                    if (vehicle.Lead != null)
                    {
                        item.CustomerName = vehicle.Lead.FullName;
                    }
                }
            }
        }
        var dtos = items
            .Select(
                x => new ChatRepairOrderListItemDto
                {
                    RepairOrderId = x.Id,
                    VehicleInfo = x.VehicleInfo,
                    CustomerName = x.CustomerName,
                    MaintenanceDate = x.MaintenanceDate
                })
            .ToList();
        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatRepairOrderListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IMaintenanceHistoryReadRepository.GetPagedAsync",
            new Dictionary<string, string>(),
            "phieu-sua-chua",
            null);
        return ChatToolEnvelope<ChatRepairOrderListItemDto>.Wrap(inner, meta);
    }
}
