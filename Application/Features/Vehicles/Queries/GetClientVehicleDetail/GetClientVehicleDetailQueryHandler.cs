using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Mapster;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetClientVehicleDetail;

public class GetClientVehicleDetailQueryHandler(
    IVehicleReadRepository vehicleReadRepository,
    IMaintenanceHistoryReadRepository maintenanceHistoryReadRepository)
    : IRequestHandler<GetClientVehicleDetailQuery, Result<VehicleResponse?>>
{
    public async Task<Result<VehicleResponse?>> Handle(
        GetClientVehicleDetailQuery request,
        CancellationToken cancellationToken)
    {
        var vehicle = await vehicleReadRepository
            .GetByUserIdAndIdAsync(request.UserId, request.VehicleId, cancellationToken)
            .ConfigureAwait(false);

        if (vehicle is null)
        {
            return Result<VehicleResponse?>.Failure(Error.NotFound("Vehicle not found.", "VehicleId"));
        }

        var response = vehicle.Adapt<VehicleResponse>();
        response.WarrantyRemainingDays = CalculateWarrantyRemainingDays(vehicle.WarrantyDate);

        var maintenanceInfo = await GetMaintenanceStatusAsync(vehicle, maintenanceHistoryReadRepository, cancellationToken)
            .ConfigureAwait(false);

        response.MaintenanceStatus = maintenanceInfo.Status;
        response.NextMaintenanceDate = maintenanceInfo.NextMaintenanceDate;
        response.NextMaintenanceOdo = maintenanceInfo.NextMaintenanceOdo;
        response.LastMaintenanceDate = maintenanceInfo.LastMaintenanceDate;

        return Result<VehicleResponse?>.Success(response);
    }

    private static int CalculateWarrantyRemainingDays(DateTimeOffset? warrantyDate)
    {
        if (!warrantyDate.HasValue)
        {
            return 0;
        }

        var remainingDays = (int)Math.Ceiling((warrantyDate.Value.Date - DateTimeOffset.UtcNow.Date).TotalDays);
        return Math.Max(0, remainingDays);
    }

    private static async Task<(string Status, DateTime? NextMaintenanceDate, double? NextMaintenanceOdo, DateTime? LastMaintenanceDate)> GetMaintenanceStatusAsync(
        Domain.Entities.Vehicle vehicle,
        IMaintenanceHistoryReadRepository maintenanceHistoryReadRepository,
        CancellationToken cancellationToken)
    {
        var nextMaintenanceDate = vehicle.NextMaintenanceDate;
        var nextMaintenanceOdo = vehicle.NextMaintenanceOdo;
        var lastMaintenanceDate = vehicle.LastMaintenanceDate;

        if (!nextMaintenanceDate.HasValue && !nextMaintenanceOdo.HasValue)
        {
            var history = await maintenanceHistoryReadRepository.GetByVehicleIdAsync(
                vehicle.Id,
                cancellationToken,
                Domain.Constants.DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);

            var latest = history.FirstOrDefault();
            if (latest is not null)
            {
                nextMaintenanceDate ??= latest.NextMaintenanceDate?.DateTime;
                nextMaintenanceOdo ??= latest.NextMaintenanceOdo;
                lastMaintenanceDate ??= latest.MaintenanceDate.DateTime;
            }
        }

        var status = DetermineMaintenanceStatus(vehicle.CurrentOdo, nextMaintenanceDate, nextMaintenanceOdo);
        return (status, nextMaintenanceDate, nextMaintenanceOdo, lastMaintenanceDate);
    }

    private static string DetermineMaintenanceStatus(double currentOdo, DateTime? nextMaintenanceDate, double? nextMaintenanceOdo)
    {
        var today = DateTime.UtcNow.Date;

        var overdueByDate = nextMaintenanceDate.HasValue && nextMaintenanceDate.Value.Date < today;
        var dueSoonByDate = nextMaintenanceDate.HasValue && nextMaintenanceDate.Value.Date <= today.AddDays(7);

        var overdueByOdo = nextMaintenanceOdo.HasValue && currentOdo >= nextMaintenanceOdo.Value;
        var dueSoonByOdo = nextMaintenanceOdo.HasValue && currentOdo >= nextMaintenanceOdo.Value - 500;

        if (overdueByDate || overdueByOdo)
        {
            return "Overdue";
        }

        if (dueSoonByDate || dueSoonByOdo)
        {
            return "DueSoon";
        }

        return "NotDue";
    }
}
