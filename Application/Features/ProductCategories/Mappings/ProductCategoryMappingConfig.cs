using Application.ApiContracts.ProductCategory.Responses;
using Application.Features.ProductCategories.Commands.CreateProductCategory;
using Application.Features.ProductCategories.Commands.UpdateProductCategory;
using Mapster;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Mappings;

public sealed class ProductCategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateProductCategoryCommand, CategoryEntity>().Map(dest => dest.Description, src => string.IsNullOrWhiteSpace(src.Description) ? null : src.Description);

        config.NewConfig<CategoryEntity, ProductCategoryResponse>();

        config.NewConfig<UpdateProductCategoryCommand, CategoryEntity>().IgnoreNullValues(true);
    }
}
