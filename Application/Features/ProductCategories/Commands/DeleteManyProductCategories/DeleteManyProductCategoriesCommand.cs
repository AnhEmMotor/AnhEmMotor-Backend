
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteManyProductCategories;

public sealed record DeleteManyProductCategoriesCommand : IRequest<Common.Models.ErrorResponse?>
{
    public List<int> Ids { get; init; } = [];
}
