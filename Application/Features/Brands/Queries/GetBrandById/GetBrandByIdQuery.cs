
using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandById;

public sealed record GetBrandByIdQuery : IRequest<Result<BrandResponse>>
{
    public int? Id { get; init; }
}
