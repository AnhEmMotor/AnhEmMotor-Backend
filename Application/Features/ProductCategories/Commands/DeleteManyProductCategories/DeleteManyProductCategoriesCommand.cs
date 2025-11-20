using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteManyProductCategories;

public sealed record DeleteManyProductCategoriesCommand(List<int> Ids) : IRequest<ErrorResponse?>;
