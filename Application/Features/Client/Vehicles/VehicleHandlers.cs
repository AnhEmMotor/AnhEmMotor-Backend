using MediatR;
using Application.ApiContracts.Client.Vehicles;
using Application.Interfaces.Repositories.Vehicle;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Application.Features.Client.Vehicles
{
    public record GetMyVehiclesQuery() : IRequest<List<VehicleSummaryResponse>>;
    public record GetVehicleDetailQuery(int Id) : IRequest<VehicleDetailResponse?>;
    public record UpdateOdoCommand(int VehicleId, double NewOdo) : IRequest<bool>;

    public class GetMyVehiclesHandler : IRequestHandler<GetMyVehiclesQuery, List<VehicleSummaryResponse>>
    {
        private readonly IVehicleReadRepository _readRepo;
        public GetMyVehiclesHandler(IVehicleReadRepository readRepo) => _readRepo = readRepo;

        public async Task<List<VehicleSummaryResponse>> Handle(GetMyVehiclesQuery request, CancellationToken cancellationToken)
        {
            // Mock current user ID
            var vehicles = await _readRepo.GetByUserIdAsync("mock-user-id", cancellationToken);
            return vehicles.Select(v => new VehicleSummaryResponse(
                v.Id, "url_to_image", v.Product?.Name ?? string.Empty, v.LicensePlate, v.CurrentOdo,
                v.NextMaintenanceDate.HasValue ? (v.NextMaintenanceDate.Value - DateTime.Now).Days : 0,
                (v.NextMaintenanceOdo ?? v.CurrentOdo) - v.CurrentOdo,
                v.ElectronicWarrantyQrCode, "Active")).ToList();
        }
    }

    public class GetVehicleDetailHandler : IRequestHandler<GetVehicleDetailQuery, VehicleDetailResponse?>
    {
        private readonly IVehicleReadRepository _readRepo;
        public GetVehicleDetailHandler(IVehicleReadRepository readRepo) => _readRepo = readRepo;

        public async Task<VehicleDetailResponse?> Handle(GetVehicleDetailQuery request, CancellationToken cancellationToken)
        {
            var vehicle = await _readRepo.GetByIdAsync(request.Id, cancellationToken);
            if (vehicle == null) return null;

            return new VehicleDetailResponse(
                vehicle.Id, vehicle.Product?.Name ?? string.Empty, vehicle.VinNumber, vehicle.LicensePlate,
                vehicle.Product?.Description ?? "No specs", 
                new List<MaintenanceHistoryDto>());
        }
    }

    public class UpdateOdoHandler : IRequestHandler<UpdateOdoCommand, bool>
    {
        private readonly IVehicleUpdateRepository _updateRepo;
        public UpdateOdoHandler(IVehicleUpdateRepository updateRepo) => _updateRepo = updateRepo;

        public async Task<bool> Handle(UpdateOdoCommand request, CancellationToken cancellationToken)
        {
            await _updateRepo.UpdateOdoAsync(request.VehicleId, request.NewOdo, cancellationToken);
            return true;
        }
    }
}
