using Application.ApiContracts.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommand(int Id, string? Name, string? Address, string? PhoneNumber, string? Email) : IRequest<(SupplierResponse? Data, ErrorResponse? Error)>;
