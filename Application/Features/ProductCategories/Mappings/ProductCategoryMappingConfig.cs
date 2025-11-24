using Application.ApiContracts.ProductCategory;
using Application.Features.ProductCategories.Commands.CreateProductCategory;
using Application.Features.ProductCategories.Commands.DeleteManyProductCategories;
using Application.Features.ProductCategories.Commands.RestoreManyProductCategories;
using Application.Features.ProductCategories.Commands.UpdateProductCategory;
using Mapster;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Mappings;

public sealed class ProductCategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateProductCategoryRequest, CreateProductCategoryCommand>();

        config.NewConfig<CreateProductCategoryCommand, CategoryEntity>();

        config.NewConfig<CategoryEntity, ProductCategoryResponse>();

        config.NewConfig<UpdateProductCategoryRequest, UpdateProductCategoryCommand>();

        config.NewConfig<UpdateProductCategoryCommand, CategoryEntity>().IgnoreNullValues(true);

        config.NewConfig<DeleteManyProductCategoriesRequest, DeleteManyProductCategoriesCommand>();

        config.NewConfig<RestoreManyProductCategoriesRequest, RestoreManyProductCategoriesCommand>();
    }
}
