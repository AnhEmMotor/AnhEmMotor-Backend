using Application.ApiContracts.Supplier.Responses;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreManySuppliers;

public sealed record RestoreManySuppliersCommand : IRequest<(List<SupplierResponse>? Data, ErrorResponse? Error)>
{
    public List<int> Ids { get; init; } = [];
}