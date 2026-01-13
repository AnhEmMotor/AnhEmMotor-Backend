
using Application.Common.Models;
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteProductCategory;

public sealed record DeleteProductCategoryCommand : IRequest<Result>
{
    public int Id { get; init; }
}
