using Application.ApiContracts.Vehicle.Responses;
using Domain.Entities;
using Mapster;

namespace Application.Features.Vehicles.Mappings;

public sealed class VehicleMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Vehicle, VehicleResponse>()
            .Map(dest => dest.FullName, src => src.Lead!.FullName)
            .Map(dest => dest.PhoneNumber, src => src.Lead!.PhoneNumber)
            .Map(dest => dest.ProductVariantId, src => src.ProductVariantId)
            .Map(dest => dest.ProductVariantColorId, src => src.ProductVariantColorId);
    }
}
