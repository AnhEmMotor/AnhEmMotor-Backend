using Application.ApiContracts.Supplier.Responses;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreManySuppliers;

public sealed record RestoreManySuppliersCommand : IRequest<(List<SupplierResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public List<int> Ids { get; init; } = [];
}