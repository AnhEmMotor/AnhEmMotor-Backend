using Application.ApiContracts.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateManySupplierStatus;

public sealed record UpdateManySupplierStatusCommand : IRequest<(List<SupplierResponse>? Data, ErrorResponse? Error)>
{
    public List<int> Ids { get; init; } = [];
    public string? StatusId { get; init; }
}