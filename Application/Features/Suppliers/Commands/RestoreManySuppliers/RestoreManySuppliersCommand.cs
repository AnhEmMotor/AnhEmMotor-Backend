using Application.ApiContracts.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreManySuppliers;

public sealed record RestoreManySuppliersCommand(List<int> Ids) : IRequest<(List<SupplierResponse>? Data, ErrorResponse? Error)>;
