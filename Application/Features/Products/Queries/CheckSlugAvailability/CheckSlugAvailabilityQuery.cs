using MediatR;
using Application.ApiContracts.Product.Responses;
using Application.Common.Models;

namespace Application.Features.Products.Queries.CheckSlugAvailability;

public sealed record CheckSlugAvailabilityQuery(string Slug) : IRequest<Result<SlugAvailabilityResponse>>;
