using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers;

public sealed record DeleteManySuppliersCommand(List<int> Ids) : IRequest<ErrorResponse?>;
