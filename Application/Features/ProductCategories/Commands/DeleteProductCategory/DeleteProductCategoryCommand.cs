using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteProductCategory;

public sealed record DeleteProductCategoryCommand(int Id) : IRequest<ErrorResponse?>;
