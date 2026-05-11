using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.TechnologyCategory.TechnologyCategory;
using Mapster;
using MediatR;

namespace Application.Features.Technologies.Queries.GetAllTechnologyCategories;

public sealed class GetAllTechnologyCategoriesQueryHandler(ITechnologyCategoryReadRepository categoryRepository) : IRequestHandler<GetAllTechnologyCategoriesQuery, Result<List<TechnologyCategoryResponse>>>
{
    public async Task<Result<List<TechnologyCategoryResponse>>> Handle(
        GetAllTechnologyCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return categories.Adapt<List<TechnologyCategoryResponse>>();
    }
}
