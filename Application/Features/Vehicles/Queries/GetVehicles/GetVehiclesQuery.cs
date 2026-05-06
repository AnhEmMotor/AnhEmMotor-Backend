using Application.Interfaces.Repositories.Vehicle;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetVehicles;

public sealed class GetVehiclesQuery : IRequest<Result<List<VehicleResponse>>>
{
    public string? Search { get; set; }
}

public class VehicleResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string VinNumber { get; set; } = string.Empty;
    public string EngineNumber { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public DateTimeOffset PurchaseDate { get; set; }
}

public sealed class GetVehiclesQueryHandler(IVehicleReadRepository vehicleRepository) 
    : IRequestHandler<GetVehiclesQuery, Result<List<VehicleResponse>>>
{
    public async Task<Result<List<VehicleResponse>>> Handle(GetVehiclesQuery request, CancellationToken cancellationToken)
    {
        var entities = await vehicleRepository.GetVehiclesAsync(request.Search, cancellationToken);

        var vehicles = entities.Select(v => new VehicleResponse
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
