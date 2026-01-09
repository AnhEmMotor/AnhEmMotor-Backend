using Application.ApiContracts.Supplier.Responses;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreSupplier;

public sealed record RestoreSupplierCommand : IRequest<(SupplierResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public int Id { get; init; }
}