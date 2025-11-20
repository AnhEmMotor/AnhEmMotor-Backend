using Application.ApiContracts.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreSupplier;

public sealed record RestoreSupplierCommand(int Id) : IRequest<(SupplierResponse? Data, ErrorResponse? Error)>;
