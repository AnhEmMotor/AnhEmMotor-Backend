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
            .Map(
                dest => dest.ColorName,
                src => src.ProductVariantColor != null 
                    ? src.ProductVariantColor.ColorName 
                    : (src.Product != null && src.Product.ProductVariants.Any() && src.Product.ProductVariants.FirstOrDefault()!.ProductVariantColors.Any() 
                        ? src.Product.ProductVariants.FirstOrDefault()!.ProductVariantColors.FirstOrDefault()!.ColorName 
                        : null))
            .Map(
                dest => dest.VariantName, 
                src => src.ProductVariant != null 
                    ? src.ProductVariant.VariantName 
                    : (src.Product != null && src.Product.ProductVariants.Any() 
                        ? src.Product.ProductVariants.FirstOrDefault()!.VariantName 
                        : null))
            .Map(
                dest => dest.BrandName,
                src => src.Product != null && src.Product.Brand != null ? src.Product.Brand.Name : null)
            .Map(
                dest => dest.CategoryName,
                src => src.Product != null && src.Product.ProductCategory != null
                    ? src.Product.ProductCategory.Name
                    : null)
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
            .Map(dest => dest.WarrantyPeriod, src => src.Product != null ? src.Product.WarrantyPeriod : null)
            .Map(
                dest => dest.ImageUrl,
                src => src.ProductVariantColor != null 
                    ? src.ProductVariantColor.CoverImageUrl 
                    : (src.Product != null && src.Product.ProductVariants.Any() && src.Product.ProductVariants.FirstOrDefault()!.ProductVariantColors.Any() 
                        ? src.Product.ProductVariants.FirstOrDefault()!.ProductVariantColors.FirstOrDefault()!.CoverImageUrl 
                        : (src.Product != null && src.Product.ProductVariants.Any() 
                            ? src.Product.ProductVariants.FirstOrDefault()!.CoverImageUrl 
                            : null)));
    }
}
