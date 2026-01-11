
using Application.Common.Models;
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteManyProductCategories;

public sealed record DeleteManyProductCategoriesCommand : IRequest<Result>
{
    public List<int> Ids { get; init; } = [];
}
