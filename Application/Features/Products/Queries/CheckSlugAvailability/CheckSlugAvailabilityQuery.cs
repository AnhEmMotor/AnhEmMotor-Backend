using MediatR;

namespace Application.Features.Products.Queries.CheckSlugAvailability;

public sealed record CheckSlugAvailabilityQuery(string Slug) : IRequest<ApiContracts.Product.Responses.SlugAvailabilityResponse>;
