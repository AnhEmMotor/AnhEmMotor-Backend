using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreSupplier;

public sealed record RestoreSupplierCommand : IRequest<Result<SupplierResponse?>>
{
    public int Id { get; init; }
}