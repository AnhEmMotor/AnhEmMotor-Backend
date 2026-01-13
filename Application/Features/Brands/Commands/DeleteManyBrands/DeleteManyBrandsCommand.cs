using Application.Common.Models;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteManyBrands;

public sealed record DeleteManyBrandsCommand : IRequest<Result>
{
    public List<int> Ids { get; init; } = [];
}
