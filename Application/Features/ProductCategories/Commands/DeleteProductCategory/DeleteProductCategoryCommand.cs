using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteProductCategory;

public sealed record DeleteProductCategoryCommand : IRequest<ErrorResponse?>
{
    public int Id { get; init; }
}
