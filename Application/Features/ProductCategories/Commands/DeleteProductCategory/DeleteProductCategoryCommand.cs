
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteProductCategory;

public sealed record DeleteProductCategoryCommand : IRequest<Common.Models.ErrorResponse?>
{
    public int Id { get; init; }
}
