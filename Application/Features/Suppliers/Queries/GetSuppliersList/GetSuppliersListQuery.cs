using Application.ApiContracts.Supplier.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Suppliers.Queries.GetSuppliersList;

public sealed record GetSuppliersListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<SupplierResponse>>;
