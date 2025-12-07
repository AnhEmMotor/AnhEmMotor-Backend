using Application.ApiContracts.Supplier.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Suppliers.Queries.GetDeletedSuppliersList;

public sealed record GetDeletedSuppliersListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<SupplierResponse>>;
