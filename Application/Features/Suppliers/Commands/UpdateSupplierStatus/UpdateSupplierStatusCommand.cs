using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateSupplierStatus;

public sealed record UpdateSupplierStatusCommand : IRequest<Result<SupplierResponse?>>
{
    public int Id { get; init; }

    public string? StatusId { get; init; }
}