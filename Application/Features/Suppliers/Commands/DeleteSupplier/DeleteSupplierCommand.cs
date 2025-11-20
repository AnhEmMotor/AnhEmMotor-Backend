using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteSupplier;

public sealed record DeleteSupplierCommand(int Id) : IRequest<ErrorResponse?>;
