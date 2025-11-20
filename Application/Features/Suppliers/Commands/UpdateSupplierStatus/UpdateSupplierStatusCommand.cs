using Application.ApiContracts.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateSupplierStatus;

public sealed record UpdateSupplierStatusCommand(int Id, string? Status) : IRequest<(SupplierResponse? Data, ErrorResponse? Error)>;
