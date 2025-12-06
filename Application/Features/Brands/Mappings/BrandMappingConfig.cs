
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
        config.NewConfig<ApiContracts.Brand.Requests.CreateBrandRequest, CreateBrandCommand>();

        config.NewConfig<CreateBrandCommand, BrandEntity>();

        config.NewConfig<BrandEntity, ApiContracts.Brand.Responses.BrandResponse>();

        config.NewConfig<ApiContracts.Brand.Requests.UpdateBrandRequest, UpdateBrandCommand>();

        config.NewConfig<UpdateBrandCommand, BrandEntity>().IgnoreNullValues(true);

        config.NewConfig<ApiContracts.Brand.Requests.DeleteManyBrandsRequest, DeleteManyBrandsCommand>();

        config.NewConfig<ApiContracts.Brand.Requests.RestoreManyBrandsRequest, RestoreManyBrandsCommand>();
    }
}