using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Vehicles.Queries.GetVehicles
{
    public sealed class GetVehiclesQueryHandler(IVehicleReadRepository vehicleRepository) : IRequestHandler<GetVehiclesQuery, Result<List<VehicleResponse>>>
    {
        public async Task<Result<List<VehicleResponse>>> Handle(
            GetVehiclesQuery request,
            CancellationToken cancellationToken)
        {
            var entities = await vehicleRepository.GetVehiclesAsync(request.Search, cancellationToken).ConfigureAwait(false);
            var vehicles = entities.Select(
                v => new VehicleResponse
                {
                    Id = v.Id,
                    FullName = v.Lead.FullName,
                    PhoneNumber = v.Lead.PhoneNumber,
                    VinNumber = v.VinNumber,
                    EngineNumber = v.EngineNumber,
                    LicensePlate = v.LicensePlate,
                    PurchaseDate = v.PurchaseDate
                })
                .ToList();
            return vehicles;
        }
    }

}
