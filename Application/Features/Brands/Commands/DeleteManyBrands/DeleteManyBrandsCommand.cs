using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteManyBrands;

public sealed record DeleteManyBrandsCommand : IRequest<ErrorResponse?>
{
    public List<int> Ids { get; init; } = [];
}
