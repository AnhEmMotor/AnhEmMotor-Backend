using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers;

public sealed record DeleteManySuppliersCommand : IRequest<ErrorResponse?>
{
    public List<int> Ids { get; init; } = [];
}