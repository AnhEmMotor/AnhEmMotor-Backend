using Domain.Common.Models;
using MediatR;

namespace Application.Features.Brands.Commands.UpdateBrand;

public sealed record UpdateBrandCommand : IRequest<(ApiContracts.Brand.Responses.BrandResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }
}
