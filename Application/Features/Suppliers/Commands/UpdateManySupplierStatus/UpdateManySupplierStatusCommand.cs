using Application.ApiContracts.Supplier.Responses;

using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateManySupplierStatus;

public sealed record UpdateManySupplierStatusCommand : IRequest<(List<SupplierResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public List<int> Ids { get; init; } = [];

    public string? StatusId { get; init; }
}