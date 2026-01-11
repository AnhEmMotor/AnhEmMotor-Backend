using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreManySuppliers;

public sealed record RestoreManySuppliersCommand : IRequest<Result<List<SupplierResponse>?>>
{
    public List<int> Ids { get; init; } = [];
}