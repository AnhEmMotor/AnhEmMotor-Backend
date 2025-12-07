using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers;

public sealed record DeleteManySuppliersCommand : IRequest<Common.Models.ErrorResponse?>
{
    public List<int> Ids { get; init; } = [];
}