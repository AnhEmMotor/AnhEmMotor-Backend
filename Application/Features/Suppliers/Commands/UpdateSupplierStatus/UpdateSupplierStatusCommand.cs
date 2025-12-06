using Application.ApiContracts.Supplier.Responses;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateSupplierStatus;

public sealed record UpdateSupplierStatusCommand : IRequest<(SupplierResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string? StatusId { get; init; }
}