using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Products.Queries.GetPersonalizedRecommendations;

public sealed record GetPersonalizedRecommendationsQuery(int PageSize, string? VisitorKey) : IRequest<Result<PagedResult<ProductListStoreResponse>>>;
