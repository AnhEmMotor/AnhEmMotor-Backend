using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.RestoreBrand;

public sealed record RestoreBrandCommand : IRequest<(ApiContracts.Brand.Responses.BrandResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }
}
