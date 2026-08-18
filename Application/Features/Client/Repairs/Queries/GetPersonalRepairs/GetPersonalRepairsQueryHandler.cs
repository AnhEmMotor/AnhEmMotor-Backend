using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Sieve.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Client.Repairs.Queries.GetPersonalRepairs;

public class GetPersonalRepairsQueryHandler : IRequestHandler<GetPersonalRepairsQuery, List<RepairOrderResponse>>
{
    private readonly IMaintenanceHistoryReadRepository _repairRepository;
    private readonly IVehicleReadRepository _vehicleRepository;

    public GetPersonalRepairsQueryHandler(IMaintenanceHistoryReadRepository repairRepository, IVehicleReadRepository vehicleRepository)
    {
        _repairRepository = repairRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<List<RepairOrderResponse>> Handle(GetPersonalRepairsQuery request, CancellationToken cancellationToken)
    {
        var sieveModel = request.SieveModel ?? new SieveModel();
        if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
        {
            sieveModel.Sorts = "-MaintenanceDate";
        }

        // Get vehicles for user
        var vehiclesResult = await _vehicleRepository.GetPagedAsync<Application.ApiContracts.Vehicle.Responses.VehicleResponse>(
            new SieveModel { PageSize = 100 },
            DataFetchMode.ActiveOnly,
            v => v.UserId == request.CurrentUserId,
            cancellationToken);

        var vehicleIds = vehiclesResult.Items?.Select(v => v.Id).ToList() ?? new List<int>();

        if (!vehicleIds.Any()) 
        {
            return new List<RepairOrderResponse>();
        }

        var repairs = await _repairRepository.GetPagedAsync<MaintenanceHistory>(
            sieveModel,
            DataFetchMode.ActiveOnly,
            o => vehicleIds.Contains(o.VehicleId),
            cancellationToken);

        if (repairs.Items == null || !repairs.Items.Any())
        {
            return new List<RepairOrderResponse>();
        }

        var responses = repairs.Items.Select(r => 
        {
            var vehicle = vehiclesResult.Items?.FirstOrDefault(v => v.Id == r.VehicleId);
            return new RepairOrderResponse
            {
                Id = r.Id,
                MaintenanceNumber = r.MaintenanceNumber,
                VehicleId = r.VehicleId,
                MaintenanceDate = r.MaintenanceDate,
                Description = r.Description,
                Mileage = r.Mileage,
                TechnicianId = r.TechnicianId,
                PartsCost = r.PartsCost,
                LaborCost = r.LaborCost,
                TotalCost = r.TotalCost,
                PartsJson = r.PartsJson,
                NextMaintenanceDate = r.NextMaintenanceDate,
                NextMaintenanceOdo = r.NextMaintenanceOdo,
                ServiceType = r.ServiceType,
                VehicleInfo = "Xe bảo dưỡng",
                ProductImage = vehicle?.ImageUrl,
                VehicleName = vehicle?.ProductName,
                CategoryName = vehicle?.CategoryName,
                VariantName = vehicle?.VariantName,
                ColorName = vehicle?.ColorName,
                VinNumber = vehicle?.VinNumber,
                ExpectedCompletionDate = r.MaintenanceDate, // Defaulting to MaintenanceDate as no ExpectedCompletionDate exists in MaintenanceHistory
            };
        }).ToList();

        return responses;
    }
}
