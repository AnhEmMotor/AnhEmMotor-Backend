using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed class GetProductCategoriesListQueryHandler(
    IProductCategoryReadRepository repository,
    ISievePaginator paginator) : IRequestHandler<GetProductCategoriesListQuery, Result<PagedResult<ProductCategoryResponse>>>
{
    public async Task<Result<PagedResult<ProductCategoryResponse>>> Handle(
        GetProductCategoriesListQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<ProductCategoryEntity> query = repository.GetQueryable().Include(x => x.Products);
        if (request.ProductOnly)
        {
            query = query.Where(x => x.CategoryGroup == "Product");
        }
        var result = await paginator.ApplyAsync<ProductCategoryEntity, ProductCategoryResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}