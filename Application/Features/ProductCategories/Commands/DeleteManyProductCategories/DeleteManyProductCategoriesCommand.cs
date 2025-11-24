using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteManyProductCategories;

public sealed record DeleteManyProductCategoriesCommand : IRequest<ErrorResponse?>
{
    public List<int> Ids { get; init; } = [];
}
