using Application.ApiContracts.Vehicle.Responses;
using Domain.Entities;
using Mapster;

namespace Application.Features.Vehicles.Mappings;

public class VehicleMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Vehicle, VehicleResponse>()
            .Map(dest => dest.FullName, src => src.Lead!.FullName)
            .Map(dest => dest.PhoneNumber, src => src.Lead!.PhoneNumber)
            .Map(dest => dest.ProductVariantId, src => src.ProductVariantId)
            .Map(dest => dest.ProductVariantColorId, src => src.ProductVariantColorId)
            .Map(dest => dest.ColorName, src => src.ProductVariantColor != null ? src.ProductVariantColor.ColorName : null)
            .Map(dest => dest.VariantName, src => src.ProductVariant != null ? src.ProductVariant.VariantName : null)
            .Map(dest => dest.BrandName, src => src.Product != null && src.Product.Brand != null ? src.Product.Brand.Name : null)
            .Map(dest => dest.WarrantyPeriod, src => src.Product != null ? src.Product.WarrantyPeriod : null);
    }
}
