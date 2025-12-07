using MediatR;

namespace Application.Features.Brands.Commands.DeleteManyBrands;

public sealed record DeleteManyBrandsCommand : IRequest<Common.Models.ErrorResponse?>
{
    public List<int> Ids { get; init; } = [];
}
