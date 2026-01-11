using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommand : IRequest<Result<SupplierResponse?>>
{
    public int Id { get; init; }

    public string? Name { get; init; }

    public string? Address { get; init; }

    public string? Phone { get; init; }

    public string? Email { get; init; }

    public string? Notes { get; init; }

    public string? TaxIdentificationNumber { get; set; }
}