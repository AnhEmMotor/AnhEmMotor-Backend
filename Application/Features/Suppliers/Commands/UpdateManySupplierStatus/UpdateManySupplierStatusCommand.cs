using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateManySupplierStatus;

public sealed record UpdateManySupplierStatusCommand : IRequest<Result<List<SupplierResponse>?>>
{
    public List<int> Ids { get; init; } = [];

    public string? StatusId { get; init; }
}