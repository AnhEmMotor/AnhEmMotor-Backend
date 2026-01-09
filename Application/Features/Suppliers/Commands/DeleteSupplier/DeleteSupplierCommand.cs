
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteSupplier;

public sealed record DeleteSupplierCommand : IRequest<Common.Models.ErrorResponse?>
{
    public int Id { get; init; }
}