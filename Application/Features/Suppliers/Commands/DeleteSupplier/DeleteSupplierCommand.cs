using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteSupplier;

public sealed record DeleteSupplierCommand : IRequest<ErrorResponse?>
{
    public int Id { get; init; }
}