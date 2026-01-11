using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSupplierById;

public sealed record GetSupplierByIdQuery(int Id) : IRequest<Result<SupplierResponse?>>;
