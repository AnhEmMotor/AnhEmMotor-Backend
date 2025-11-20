using Application.ApiContracts.Brand;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandById;

public sealed record GetBrandByIdQuery(int Id) : IRequest<(BrandResponse? Data, ErrorResponse? Error)>;
