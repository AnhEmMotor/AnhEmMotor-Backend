using Application.ApiContracts.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateManySupplierStatus;

public sealed record UpdateManySupplierStatusCommand(Dictionary<int, string> Updates) : IRequest<(List<SupplierResponse>? Data, ErrorResponse? Error)>;
