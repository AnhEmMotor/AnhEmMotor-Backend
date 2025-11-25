using Application.ApiContracts.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommand : IRequest<(SupplierResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string? Name { get; init; }

    public string? Address { get; init; }

    public string? Phone { get; init; }

    public string? Email { get; init; }

    public string? Notes { get; init; }
}