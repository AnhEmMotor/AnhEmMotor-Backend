using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.News.Queries.GetProductsForNews;

public class GetProductsForNewsQuery : IRequest<Result<PagedResult<ProductNewsResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}

