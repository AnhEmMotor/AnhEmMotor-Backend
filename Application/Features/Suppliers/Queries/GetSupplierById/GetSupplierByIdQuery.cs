using Application.ApiContracts.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSupplierById;

public sealed record GetSupplierByIdQuery(int Id) : IRequest<(SupplierResponse? Data, ErrorResponse? Error)>;
