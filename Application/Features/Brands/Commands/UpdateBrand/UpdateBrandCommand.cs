using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.UpdateBrand;

public sealed record UpdateBrandCommand : IRequest<(ApiContracts.Brand.Responses.BrandResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }
}
