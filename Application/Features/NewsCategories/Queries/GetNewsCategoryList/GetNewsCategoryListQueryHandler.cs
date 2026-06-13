using Application.ApiContracts.NewsCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.NewsCategory;
using Domain.Primitives;
using MediatR;

namespace Application.Features.NewsCategories.Queries.GetNewsCategoryList;

public class GetNewsCategoryListQueryHandler(INewsCategoryReadRepository repository) : IRequestHandler<GetNewsCategoryListQuery, Result<PagedResult<NewsCategoryResponse>>>
{
    public async Task<Result<PagedResult<NewsCategoryResponse>>> Handle(
        GetNewsCategoryListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<NewsCategoryResponse>(
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
