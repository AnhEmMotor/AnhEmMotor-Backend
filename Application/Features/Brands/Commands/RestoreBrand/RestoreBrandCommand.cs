using Application.ApiContracts.Brand;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreBrand;

public sealed record RestoreBrandCommand : IRequest<(BrandResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }
}
