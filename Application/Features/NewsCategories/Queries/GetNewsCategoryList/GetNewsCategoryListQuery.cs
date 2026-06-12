using Application.ApiContracts.NewsCategory.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.NewsCategories.Queries.GetNewsCategoryList;

public sealed record GetNewsCategoryListQuery : IRequest<Result<PagedResult<NewsCategoryResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
