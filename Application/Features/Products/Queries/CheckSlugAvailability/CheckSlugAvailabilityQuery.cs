using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.CheckSlugAvailability;

public sealed record CheckSlugAvailabilityQuery : IRequest<Result<SlugAvailabilityResponse>>
{
    public string? Slug { get; init; }
}
