
using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.UpdateBrand;
using Mapster;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Mappings;

public sealed class BrandMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateBrandCommand, BrandEntity>();

        config.NewConfig<BrandEntity, ApiContracts.Brand.Responses.BrandResponse>();

        config.NewConfig<UpdateBrandCommand, BrandEntity>().IgnoreNullValues(true);
    }
}