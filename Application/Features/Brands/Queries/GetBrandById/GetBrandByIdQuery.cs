using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandById;

public sealed record GetBrandByIdQuery(int Id) : IRequest<(ApiContracts.Brand.Responses.BrandResponse? Data, ErrorResponse? Error)>;
