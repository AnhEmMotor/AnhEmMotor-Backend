using Application.ApiContracts.Supplier.Responses;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreSupplier;

public sealed record RestoreSupplierCommand : IRequest<(SupplierResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }
}