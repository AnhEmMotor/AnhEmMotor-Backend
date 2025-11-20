using Application.ApiContracts.Product.Common;
using MediatR;

namespace Application.Features.Products.Queries.CheckSlugAvailability;

public sealed record CheckSlugAvailabilityQuery(string Slug) : IRequest<SlugAvailabilityResponse>;
