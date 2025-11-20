using Application.ApiContracts.Supplier;
using MediatR;

namespace Application.Features.Suppliers.Commands.CreateSupplier;

public sealed record CreateSupplierCommand(string? Name, string? Address, string? PhoneNumber, string? Email, string? Status) : IRequest<SupplierResponse>;
