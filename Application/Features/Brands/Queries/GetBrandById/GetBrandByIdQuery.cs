using Domain.Common.Models;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandById;

public sealed record GetBrandByIdQuery(int Id) : IRequest<(ApiContracts.Brand.Responses.BrandResponse? Data, Common.Models.ErrorResponse? Error)>;
