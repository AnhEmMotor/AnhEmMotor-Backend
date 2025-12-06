using MediatR;

namespace Application.Features.Brands.Commands.CreateBrand;

public sealed record CreateBrandCommand : IRequest<ApiContracts.Brand.Responses.BrandResponse>
{
    public string? Name { get; init; }

    public string? Description { get; init; }
}
