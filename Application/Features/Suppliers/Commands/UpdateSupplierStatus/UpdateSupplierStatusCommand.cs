using Application.ApiContracts.Supplier.Responses;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateSupplierStatus;

public sealed record UpdateSupplierStatusCommand : IRequest<(SupplierResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string? StatusId { get; init; }
}