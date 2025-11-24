using Application.ApiContracts.Brand;
using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.DeleteManyBrands;
using Application.Features.Brands.Commands.RestoreManyBrands;
using Application.Features.Brands.Commands.UpdateBrand;
using Mapster;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Mappings;

public sealed class BrandMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateBrandRequest, CreateBrandCommand>();

        config.NewConfig<CreateBrandCommand, BrandEntity>();

        config.NewConfig<BrandEntity, BrandResponse>();

        config.NewConfig<UpdateBrandRequest, UpdateBrandCommand>();

        config.NewConfig<UpdateBrandCommand, BrandEntity>().IgnoreNullValues(true);

        config.NewConfig<DeleteManyBrandsRequest, DeleteManyBrandsCommand>();

        config.NewConfig<RestoreManyBrandsRequest, RestoreManyBrandsCommand>();
    }
}